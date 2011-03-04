using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.LockFile;
using Orchard.Indexing.Models;
using Orchard.Indexing.Settings;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Contains the logic which is regularly executed to retrieve index information from multiple content handlers.
    /// </summary>
    /// <remarks>
    /// This class is synchronized using a lock file as both command line and web workers can potentially use it,
    /// and singleton locks would not be shared accross those two.
    /// </remarks>
    [UsedImplicitly]
    public class IndexingTaskExecutor : IIndexNotifierHandler, IIndexStatisticsProvider {
        private readonly IRepository<IndexingTaskRecord> _repository;
        private IIndexProvider _indexProvider;
        private readonly IIndexManager _indexManager;
        private readonly IContentManager _contentManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly ILockFileManager _lockFileManager;
        private readonly IClock _clock;
        private const int ContentItemsPerLoop = 100;
        private IndexingStatus _indexingStatus = IndexingStatus.Idle;

        public IndexingTaskExecutor(
            IRepository<IndexingTaskRecord> repository,
            IIndexManager indexManager,
            IContentManager contentManager,
            IAppDataFolder appDataFolder,
            ShellSettings shellSettings,
            ILockFileManager lockFileManager,
            IClock clock) {
            _repository = repository;
            _indexManager = indexManager;
            _contentManager = contentManager;
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _lockFileManager = lockFileManager;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void UpdateIndex(string indexName) {
            ILockFile lockFile = null;
            var settingsFilename = GetSettingsFileName(indexName);
            var lockFilename = settingsFilename + ".lock";

            // acquire a lock file on the index
            if (!_lockFileManager.TryAcquireLock(lockFilename, ref lockFile)) {
                Logger.Information("Index was requested but is already running");
                return;
            }

            using (lockFile)
            {
                if (!_indexManager.HasIndexProvider()) {
                    return;
                }

                // load index settings to know what is the current state of indexing
                var indexSettings = LoadSettings(indexName);

                _indexProvider = _indexManager.GetSearchIndexProvider();

                // should the index be rebuilt
                if (!_indexProvider.Exists(indexName)) {
                    _indexProvider.CreateIndex(indexName);
                    indexSettings = new IndexSettings();
                }

                // execute indexing commands by batch of [ContentItemsPerLoop] content items
                for (; ; ){
                    var addToIndex = new List<IDocumentIndex>();
                    var deleteFromIndex = new List<int>();

                    // Rebuilding the index ?
                    if (indexSettings.Mode == IndexingMode.Rebuild) {
                        Logger.Information("Rebuilding index");
                        _indexingStatus = IndexingStatus.Rebuilding;

                        // store the last inserted task
                        var lastIndexId = _repository
                            .Fetch(x => true)
                            .OrderByDescending(x => x.Id)
                            .Select(x => x.Id)
                            .FirstOrDefault();

                        // load all content items
                        var contentItemIds = _contentManager
                            .Query(VersionOptions.Published)
                            .List()
                            .Where(x => x.Id > indexSettings.LastContentId)
                            .OrderBy(x => x.Id)
                            .Select(x => x.Id)
                            .Distinct()
                            .Take(ContentItemsPerLoop)
                            .ToArray();

                        indexSettings.LastIndexedId = lastIndexId;

                        // if no more elements to index, switch to update mode
                        if (contentItemIds.Length == 0) {
                            indexSettings.Mode = IndexingMode.Update;
                        }

                        foreach (var id in contentItemIds) {
                            try {
                                IDocumentIndex documentIndex = ExtractDocumentIndex(id);

                                if (documentIndex != null && documentIndex.IsDirty) {
                                    addToIndex.Add(documentIndex);
                                }

                                // store the last processed element
                                indexSettings.LastContentId = contentItemIds.LastOrDefault();
                            }
                            catch (Exception ex) {
                                Logger.Warning(ex, "Unable to index content item #{0} during rebuild", id);
                            }
                        }
                    }

                    if (indexSettings.Mode == IndexingMode.Update) {
                        Logger.Information("Updating index");
                        _indexingStatus = IndexingStatus.Updating;

                        // load next content items to index, by filtering and ordering on the task id
                        var lastIndexId = _repository
                            .Fetch(x => x.Id > indexSettings.LastIndexedId)
                            .OrderByDescending(x => x.Id)
                            .Select(x => x.Id)
                            .FirstOrDefault();

                        var contentItemIds = _repository
                            .Fetch(x => x.Id > indexSettings.LastIndexedId)
                            .OrderBy(x => x.Id)
                            .Take(ContentItemsPerLoop)
                            .Select(x => x.ContentItemRecord.Id)
                            .Distinct() // don't process the same content item twice
                            .ToArray();

                        indexSettings.LastIndexedId = lastIndexId;

                        foreach (var id in contentItemIds) {
                            try {
                                IDocumentIndex documentIndex = ExtractDocumentIndex(id);

                                if (documentIndex == null) {
                                    deleteFromIndex.Add(id);
                                }
                                else if (documentIndex.IsDirty) {
                                    addToIndex.Add(documentIndex);
                                }
                            }
                            catch (Exception ex) {
                                Logger.Warning(ex, "Unable to index content item #{0} during rebuild", id);
                            }
                        }
                    }

                    // save current state of the index
                    indexSettings.LastIndexedUtc = _clock.UtcNow;
                    _appDataFolder.CreateFile(settingsFilename, indexSettings.ToString());

                    if (deleteFromIndex.Count == 0 && addToIndex.Count == 0) {
                        // nothing more to do
                        _indexingStatus = IndexingStatus.Idle;
                        return;
                    }

                    // save new and updated documents to the index
                    try {
                        if (addToIndex.Count > 0) {
                            _indexProvider.Store(indexName, addToIndex);
                            Logger.Information("Added content items to index: {0}", addToIndex.Count);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "An error occured while adding a document to the index");
                    }

                    // removing documents from the index
                    try {
                        if (deleteFromIndex.Count > 0) {
                            _indexProvider.Delete(indexName, deleteFromIndex);
                            Logger.Information("Added content items to index: {0}", addToIndex.Count);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "An error occured while removing a document from the index");
                    }
                }
            }
        }

        /// <summary>
        /// Loads the settings file or create a new default one if it doesn't exist
        /// </summary>
        public IndexSettings LoadSettings(string indexName) {
            var indexSettings = new IndexSettings();
            var settingsFilename = GetSettingsFileName(indexName);
            if (_appDataFolder.FileExists(settingsFilename)) {
                var content = _appDataFolder.ReadFile(settingsFilename);
                indexSettings = IndexSettings.Parse(content);
            }

            return indexSettings;
        }

        /// <summary>
        /// Creates a IDocumentIndex instance for a specific content item id. If the content 
        /// item is no more published, it returns null.
        /// </summary>
        private IDocumentIndex ExtractDocumentIndex(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Published);

            // ignore deleted or unpublished items
            if(contentItem == null || !contentItem.IsPublished()) {
                return null;
            }

            // skip items from types which are not indexed
            var settings = GetTypeIndexingSettings(contentItem);
            if (!settings.Included)
                return null;

            var documentIndex = _indexProvider.New(contentItem.Id);

            // call all handlers to add content to index
            _contentManager.Index(contentItem, documentIndex);
            return documentIndex;
        }

        static TypeIndexing GetTypeIndexingSettings(ContentItem contentItem) {
            if (contentItem == null ||
                contentItem.TypeDefinition == null ||
                contentItem.TypeDefinition.Settings == null) {
                return new TypeIndexing { Included = false };
            }
            return contentItem.TypeDefinition.Settings.GetModel<TypeIndexing>();
        }

        private string GetSettingsFileName(string indexName) {
            return _appDataFolder.Combine("Sites", _shellSettings.Name, indexName + ".settings.xml");
        }

        public DateTime GetLastIndexedUtc(string indexName) {
            var indexSettings = LoadSettings(indexName);
            return indexSettings.LastIndexedUtc;
        }

        public IndexingStatus GetIndexingStatus(string indexName) {
            return _indexingStatus;
        }
    }
}
