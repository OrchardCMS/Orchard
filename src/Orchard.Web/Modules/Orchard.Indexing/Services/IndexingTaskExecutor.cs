using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
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
    public class IndexingTaskExecutor : IIndexingTaskExecutor, IIndexStatisticsProvider
    {
        private readonly IRepository<IndexingTaskRecord> _taskRepository;
        private readonly IRepository<ContentItemVersionRecord> _contentRepository;
        private IIndexProvider _indexProvider;
        private readonly IIndexManager _indexManager;
        private readonly IContentManager _contentManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly ILockFileManager _lockFileManager;
        private readonly IClock _clock;
        private readonly ITransactionManager _transactionManager;
        private const int ContentItemsPerLoop = 50;
        private IndexingStatus _indexingStatus = IndexingStatus.Idle;

        public IndexingTaskExecutor(
            IRepository<IndexingTaskRecord> taskRepository,
            IRepository<ContentItemVersionRecord> contentRepository,
            IIndexManager indexManager,
            IContentManager contentManager,
            IAppDataFolder appDataFolder,
            ShellSettings shellSettings,
            ILockFileManager lockFileManager,
            IClock clock,
            ITransactionManager transactionManager) {
            _taskRepository = taskRepository;
            _contentRepository = contentRepository;
            _indexManager = indexManager;
            _contentManager = contentManager;
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _lockFileManager = lockFileManager;
            _transactionManager = transactionManager;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool RebuildIndex(string indexName) {

            if (DeleteIndex(indexName)) {
                var searchProvider = _indexManager.GetSearchIndexProvider();
                searchProvider.CreateIndex(indexName);
                return true;
            }

            return false;
        }

        public bool DeleteIndex(string indexName) {
            ILockFile lockFile = null;
            var settingsFilename = GetSettingsFileName(indexName);
            var lockFilename = settingsFilename + ".lock";

            // acquire a lock file on the index
            if (!_lockFileManager.TryAcquireLock(lockFilename, ref lockFile)) {
                Logger.Information("Could not delete the index. Already in use.");
                return false;
            }

            using (lockFile) {
                if (!_indexManager.HasIndexProvider()) {
                    return false;
                }

                var searchProvider = _indexManager.GetSearchIndexProvider();
                if (searchProvider.Exists(indexName)) {
                    searchProvider.DeleteIndex(indexName);
                }

                DeleteSettings(indexName);
            }

            return true;
        }

        public bool UpdateIndexBatch(string indexName) {
            ILockFile lockFile = null;
            var settingsFilename = GetSettingsFileName(indexName);
            var lockFilename = settingsFilename + ".lock";

            // acquire a lock file on the index
            if (!_lockFileManager.TryAcquireLock(lockFilename, ref lockFile)) {
                Logger.Information("Index was requested but is already running");
                return false;
            }

            using (lockFile) {
                if (!_indexManager.HasIndexProvider()) {
                    return false;
                }

                // load index settings to know what is the current state of indexing
                var indexSettings = LoadSettings(indexName);

                _indexProvider = _indexManager.GetSearchIndexProvider();

                if (indexSettings.Mode == IndexingMode.Rebuild && indexSettings.LastContentId == 0) {
                    _indexProvider.CreateIndex(indexName);

                    // mark the last available task at the moment the process is started.
                    // once the Rebuild is done, Update will start at this point of the table
                    indexSettings.LastIndexedId = _taskRepository
                        .Table
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.Id)
                        .FirstOrDefault();
                }

                // execute indexing commands by batch of [ContentItemsPerLoop] content items
                return BatchIndex(indexName, settingsFilename, indexSettings);
            }
        }

        /// <summary>
        /// Indexes a batch of content items
        /// </summary>
        /// <returns>
        /// <c>true</c> if there are more items to process; otherwise, <c>false</c>.
        /// </returns>
        private bool BatchIndex(string indexName, string settingsFilename, IndexSettings indexSettings) {
            var addToIndex = new List<IDocumentIndex>();
            var deleteFromIndex = new List<int>();
            bool loop = false;

            // Rebuilding the index ?
            if (indexSettings.Mode == IndexingMode.Rebuild) {
                Logger.Information("Rebuilding index");
                _indexingStatus = IndexingStatus.Rebuilding;

                do {
                    loop = true;

                    // load all content items
                    var contentItems = _contentRepository
                    .Table.Where(versionRecord => versionRecord.Latest && versionRecord.Id > indexSettings.LastContentId)
                        .OrderBy(versionRecord => versionRecord.Id)
                        .Take(ContentItemsPerLoop)
                        .ToList()
                        // In some rare cases a ContentItemRecord without a ContentType can end up in the DB.
                        // We need to filter out such records, otherwise they will crash the ContentManager.
                        .Where(x => x.ContentItemRecord != null && x.ContentItemRecord.ContentType != null)
                        .Select(versionRecord => _contentManager.Get(versionRecord.ContentItemRecord.Id, VersionOptions.VersionRecord(versionRecord.Id)))
                        .Distinct()
                        .ToList();

                    // if no more elements to index, switch to update mode
                    if (contentItems.Count == 0) {
                        indexSettings.Mode = IndexingMode.Update;
                    }

                    foreach (var item in contentItems) {
                        try {

                            var settings = GetTypeIndexingSettings(item);

                            // skip items from types which are not indexed
                            if (settings.List.Contains(indexName)) {
                                if (item.HasPublished()) {
                                    var published = _contentManager.Get(item.Id, VersionOptions.Published);
                                    IDocumentIndex documentIndex = ExtractDocumentIndex(published);

                                    if (documentIndex != null && documentIndex.IsDirty) {
                                        addToIndex.Add(documentIndex);
                                    }
                                }
                            }
                            else if (settings.List.Contains(indexName + ":latest")) {
                                IDocumentIndex documentIndex = ExtractDocumentIndex(item);

                                if (documentIndex != null && documentIndex.IsDirty) {
                                    addToIndex.Add(documentIndex);
                                }
                            }

                            indexSettings.LastContentId = item.VersionRecord.Id;
                        }
                        catch (Exception ex) {
                            Logger.Warning(ex, "Unable to index content item #{0} during rebuild", item.Id);
                        }
                    }

                    if (contentItems.Count < ContentItemsPerLoop) {
                        loop = false;
                    }
                    else {
                        _transactionManager.RequireNew();
                    }

                } while (loop);
            }

            if (indexSettings.Mode == IndexingMode.Update) {
                Logger.Information("Updating index");
                _indexingStatus = IndexingStatus.Updating;

                do {
                    var indexingTasks = _taskRepository
                        .Table.Where(x => x.Id > indexSettings.LastIndexedId)
                        .OrderBy(x => x.Id)
                        .Take(ContentItemsPerLoop)
                        .ToList()
                        // In some rare cases a ContentItemRecord without a ContentType can end up in the DB.
                        // We need to filter out such records, otherwise they will crash the ContentManager.
                        .Where(x => x.ContentItemRecord != null && x.ContentItemRecord.ContentType != null)
                        .GroupBy(x => x.ContentItemRecord.Id)
                    .Select(group => new { TaskId = group.Max(task => task.Id), Delete = group.Last().Action == IndexingTaskRecord.Delete, Id = group.Key, ContentItem = _contentManager.Get(group.Key, VersionOptions.Latest) })
                        .OrderBy(x => x.TaskId)
                        .ToArray();

                    foreach (var item in indexingTasks) {
                        try {

                            IDocumentIndex documentIndex = null;

                            // item.ContentItem can be null if the content item has been deleted
                            if (item.ContentItem != null) {
                                // skip items from types which are not indexed
                                var settings = GetTypeIndexingSettings(item.ContentItem);
                                if (settings.List.Contains(indexName)) {
                                    if (item.ContentItem.HasPublished()) {
                                        var published = _contentManager.Get(item.Id, VersionOptions.Published);
                                        documentIndex = ExtractDocumentIndex(published);
                                    }
                                }
                                else if (settings.List.Contains(indexName + ":latest")) {
                                    var latest = _contentManager.Get(item.Id, VersionOptions.Latest);
                                    documentIndex = ExtractDocumentIndex(latest);
                                }
                            }

                            if (documentIndex == null || item.Delete) {
                                deleteFromIndex.Add(item.Id);
                            }
                            else if (documentIndex.IsDirty) {
                                addToIndex.Add(documentIndex);
                            }

                            indexSettings.LastIndexedId = item.TaskId;
                        }
                        catch (Exception ex) {
                            Logger.Warning(ex, "Unable to index content item #{0} during update", item.Id);
                        }
                    }

                    if (indexingTasks.Length < ContentItemsPerLoop) {
                        loop = false;
                    }
                    else {
                        _transactionManager.RequireNew();
                    }

                } while (loop);
            }

            // save current state of the index
            indexSettings.LastIndexedUtc = _clock.UtcNow;
            _appDataFolder.CreateFile(settingsFilename, indexSettings.ToXml());

            if (deleteFromIndex.Count == 0 && addToIndex.Count == 0) {
                // nothing more to do
                _indexingStatus = IndexingStatus.Idle;
                return false;
            }

            // save new and updated documents to the index
            try {
                if (addToIndex.Count > 0) {
                    _indexProvider.Store(indexName, addToIndex);
                    Logger.Information("Added content items to index: {0}", addToIndex.Count);
                }
            }
            catch (Exception ex) {
                Logger.Warning(ex, "An error occurred while adding a document to the index");
            }

            // removing documents from the index
            try {
                if (deleteFromIndex.Count > 0) {
                    _indexProvider.Delete(indexName, deleteFromIndex);
                    Logger.Information("Deleted content items from index: {0}",  deleteFromIndex.Count);
                }
            }
            catch (Exception ex) {
                Logger.Warning(ex, "An error occurred while removing a document from the index");
            }

            return true;
        }

        /// <summary>
        /// Loads the settings file or create a new default one if it doesn't exist
        /// </summary>
        public IndexSettings LoadSettings(string indexName)
        {
            var indexSettings = new IndexSettings();
            var settingsFilename = GetSettingsFileName(indexName);
            if (_appDataFolder.FileExists(settingsFilename))
            {
                var content = _appDataFolder.ReadFile(settingsFilename);
                indexSettings = IndexSettings.Parse(content);
            }

            return indexSettings;
        }

        /// <summary>
        /// Deletes the settings file
        /// </summary>
        public void DeleteSettings(string indexName) {
            var settingsFilename = GetSettingsFileName(indexName);
            if (_appDataFolder.FileExists(settingsFilename)) {
                _appDataFolder.DeleteFile(settingsFilename);
            }
        }

        /// <summary>
        /// Creates a IDocumentIndex instance for a specific content item id. If the content
        /// item is no more published, it returns null.
        /// </summary>
        private IDocumentIndex ExtractDocumentIndex(ContentItem contentItem) {
            // ignore deleted or unpublished items
            if (contentItem == null || (!contentItem.IsPublished() && !contentItem.HasDraft())) {
                return null;
            }

            var documentIndex = _indexProvider.New(contentItem.Id);

            // call all handlers to add content to index
            _contentManager.Index(contentItem, documentIndex);
            return documentIndex;
        }

        private static TypeIndexing GetTypeIndexingSettings(ContentItem contentItem) {
            if (contentItem == null ||
                contentItem.TypeDefinition == null ||
                contentItem.TypeDefinition.Settings == null) {
                return new TypeIndexing {Indexes = ""};
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
