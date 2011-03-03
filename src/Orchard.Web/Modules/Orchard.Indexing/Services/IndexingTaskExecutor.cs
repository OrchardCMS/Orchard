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
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Contains the logic which is regularly executed to retrieve index information from multiple content handlers.
    /// </summary>
    /// <remarks>
    /// This class is synchronized using a lock file as both command line and web workers can potentially use it,
    /// and singleton locks would not be shared accross those two.
    /// </remarks>
    [UsedImplicitly]
    public class IndexingTaskExecutor : IIndexNotifierHandler {
        private readonly IClock _clock;
        private readonly IRepository<IndexingTaskRecord> _repository;
        private IIndexProvider _indexProvider;
        private readonly IIndexManager _indexManager;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly ILockFileManager _lockFileManager;

        public IndexingTaskExecutor(
            IClock clock,
            IRepository<IndexingTaskRecord> repository,
            IIndexManager indexManager,
            IIndexingTaskManager indexingTaskManager,
            IContentManager contentManager,
            IAppDataFolder appDataFolder,
            ShellSettings shellSettings,
            ILockFileManager lockFileManager) {
            _clock = clock;
            _repository = repository;
            _indexManager = indexManager;
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
            _appDataFolder = appDataFolder;
            _shellSettings = shellSettings;
            _lockFileManager = lockFileManager;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void UpdateIndex(string indexName) {
            ILockFile lockFile = null;
            var settingsFilename = GetSettingsFileName(indexName);
            var lockFilename = settingsFilename + ".lock";

            if (!_lockFileManager.TryAcquireLock(lockFilename, ref lockFile)) {
                Logger.Information("Index was requested but was already running");
                return;
            }

            using (lockFile) {
                if (!_indexManager.HasIndexProvider()) {
                    return;
                }

                _indexProvider = _indexManager.GetSearchIndexProvider();
                var updateIndexDocuments = new List<IDocumentIndex>();
                var addedContentItemIds = new List<string>();
                DateTime? lastIndexUtc;

                // Do we need to rebuild the full index (first time module is used, or rebuild index requested) ?
                if (_indexProvider.IsEmpty(indexName)) {
                    Logger.Information("Rebuild index started");

                    // mark current last task, as we should process older ones (in case of rebuild index only)
                    lastIndexUtc = _indexingTaskManager.GetLastTaskDateTime();

                    // get every existing content item to index it
                    foreach (var contentItem in _contentManager.Query(VersionOptions.Published).List()) {
                        try {
                            // skip items which are not indexed
                            var settings = GetTypeIndexingSettings(contentItem);
                            if (!settings.Included)
                                continue;

                            var documentIndex = _indexProvider.New(contentItem.Id);

                            _contentManager.Index(contentItem, documentIndex);
                            if (documentIndex.IsDirty) {
                                updateIndexDocuments.Add(documentIndex);
                                addedContentItemIds.Add(contentItem.Id.ToString());
                            }
                        }
                        catch (Exception ex) {
                            Logger.Warning(ex, "Unable to index content item #{0} during rebuild", contentItem.Id);
                        }
                    }

                }
                else {
                    // retrieve last processed index time
                    lastIndexUtc = _indexProvider.GetLastIndexUtc(indexName);
                }

                _indexProvider.SetLastIndexUtc(indexName, _clock.UtcNow);

                // retrieve not yet processed tasks
                var taskRecords = lastIndexUtc == null
                                      ? _repository.Fetch(x => true).ToArray()
                                      : _repository.Fetch(x => x.CreatedUtc >= lastIndexUtc).ToArray(); // CreatedUtc and lastIndexUtc might be equal if a content item is created in a background task

                // nothing to do ?)))
                if (taskRecords.Length + updateIndexDocuments.Count == 0) {
                    Logger.Information("Index update requested, nothing to do");
                    return;
                }

                Logger.Information("Processing {0} indexing tasks", taskRecords.Length);

                if (!_indexProvider.Exists(indexName)) {
                    _indexProvider.CreateIndex(indexName);
                }

                // process Delete tasks
                try {
                    var deleteIds = taskRecords.Where(t => t.Action == IndexingTaskRecord.Delete).Select(t => t.ContentItemRecord.Id).ToArray();
                    if (deleteIds.Length > 0) {
                        _indexProvider.Delete(indexName, deleteIds);
                        Logger.Information("Deleted content items from index: {0}", String.Join(", ", deleteIds));
                    }
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "An error occured while removing a document from the index");
                }

                // process Update tasks
                foreach (var taskRecord in taskRecords.Where(t => t.Action == IndexingTaskRecord.Update)) {
                    var task = new IndexingTask(_contentManager, taskRecord);

                    // skip items which are not indexed
                    var settings = GetTypeIndexingSettings(task.ContentItem);
                    if (!settings.Included)
                        continue;

                    try {
                        var documentIndex = _indexProvider.New(task.ContentItem.Id);
                        _contentManager.Index(task.ContentItem, documentIndex);
                        if (!addedContentItemIds.Contains(task.ContentItem.Id.ToString()) && documentIndex.IsDirty) {
                            updateIndexDocuments.Add(documentIndex);
                        }

                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "Unable to process indexing task #{0}", taskRecord.Id);
                    }
                }

                if (updateIndexDocuments.Count > 0) {
                    try {
                        _indexProvider.Store(indexName, updateIndexDocuments);
                        Logger.Information("Added content items to index: {0}", String.Join(", ", addedContentItemIds));
                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "An error occured while adding a document to the index");
                    }
                }
            }
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
    }
}
