using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Indexing.Models;
using Orchard.Indexing.Settings;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Services {
    /// <summary>
    /// Contains the logic which is regularly executed to retrieve index information from multiple content handlers.
    /// </summary>
    [UsedImplicitly]
    public class IndexingTaskExecutor : IIndexNotifierHandler {
        private readonly IClock _clock;
        private readonly IRepository<IndexingTaskRecord> _repository;
        private IIndexProvider _indexProvider;
        private readonly IIndexManager _indexManager;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;
        private readonly IIndexSynLock _indexSynLock;
        private const string SearchIndexName = "Search";

        public IndexingTaskExecutor(
            IClock clock,
            IRepository<IndexingTaskRecord> repository,
            IIndexManager indexManager,
            IIndexingTaskManager indexingTaskManager,
            IContentManager contentManager,
            IIndexSynLock indexSynLock) {
            _clock = clock;
            _repository = repository;
            _indexManager = indexManager;
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
            _indexSynLock = indexSynLock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void UpdateIndex(string indexName) {
            var synLock = _indexSynLock.GetSynLock(SearchIndexName);

            if (!System.Threading.Monitor.TryEnter(synLock)) {
                Logger.Information("Index was requested but was already running");
                return;
            }

            try {

                if (!_indexManager.HasIndexProvider()) {
                    return;
                }

                _indexProvider = _indexManager.GetSearchIndexProvider();
                var updateIndexDocuments = new List<IDocumentIndex>();
                DateTime lastIndexing;

                // Do we need to rebuild the full index (first time module is used, or rebuild index requested) ?
                if (_indexProvider.IsEmpty(SearchIndexName)) {
                    Logger.Information("Rebuild index started");

                    // mark current last task, as we should process older ones (in case of rebuild index only)
                    lastIndexing = _indexingTaskManager.GetLastTaskDateTime();

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
                            }
                        }
                        catch (Exception ex) {
                            Logger.Warning(ex, "Unable to index content item #{0} during rebuild", contentItem.Id);
                        }
                    }

                }
                else {
                    // retrieve last processed index time
                    lastIndexing = _indexProvider.GetLastIndexUtc(SearchIndexName);
                }

                _indexProvider.SetLastIndexUtc(SearchIndexName, _clock.UtcNow);

                // retrieve not yet processed tasks
                var taskRecords = lastIndexing == DateTime.MinValue
                    ? _repository.Fetch(x => true).ToArray()
                    : _repository.Fetch(x => x.CreatedUtc > lastIndexing).ToArray();


                // nothing to do ?
                if (taskRecords.Length + updateIndexDocuments.Count == 0)
                    return;

                Logger.Information("Processing {0} indexing tasks", taskRecords.Length);

                if (!_indexProvider.Exists(SearchIndexName)) {
                    _indexProvider.CreateIndex(SearchIndexName);
                }

                // process Delete tasks
                try {
                    _indexProvider.Delete(SearchIndexName, taskRecords.Where(t => t.Action == IndexingTaskRecord.Delete).Select(t => t.Id));
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
                        if (documentIndex.IsDirty) {
                            updateIndexDocuments.Add(documentIndex);
                        }

                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "Unable to process indexing task #{0}", taskRecord.Id);
                    }
                }

                if (updateIndexDocuments.Count > 0) {
                    try {
                        _indexProvider.Store(SearchIndexName, updateIndexDocuments);
                    }
                    catch (Exception ex) {
                        Logger.Warning(ex, "An error occured while adding a document to the index");
                    }
                }
            }
            finally {
                System.Threading.Monitor.Exit(synLock);
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
    }
}
