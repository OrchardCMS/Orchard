using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Indexing;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Core.Indexing.Models;

namespace Orchard.Core.Indexing.Services {
    /// <summary>
    /// Contains the logic which is regularly executed to retrieve index information from multiple content handlers.
    /// </summary>
    [UsedImplicitly]
    public class IndexingTaskExecutor : IBackgroundTask {
        private readonly IClock _clock;
        private readonly IRepository<IndexingTaskRecord> _repository;
        private readonly IEnumerable<IContentHandler> _handlers;
        private IIndexProvider _indexProvider;
        private readonly IIndexManager _indexManager;
        private readonly IContentManager _contentManager;
        private const string SearchIndexName = "Search";

        public IndexingTaskExecutor(
            IClock clock,
            IRepository<IndexingTaskRecord> repository,
            IEnumerable<IContentHandler> handlers,
            IIndexManager indexManager,
            IContentManager contentManager) {
            _clock = clock;
            _repository = repository;
            _indexManager = indexManager;
            _handlers = handlers;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {

            if (!_indexManager.HasIndexProvider()) {
                return;
            }

            _indexProvider = _indexManager.GetSearchIndexProvider();

            // retrieve last processed index time
            var lastIndexing = _indexProvider.GetLastIndexUtc(SearchIndexName);
            _indexProvider.SetLastIndexUtc(SearchIndexName, _clock.UtcNow);

            // retrieve not yet processed tasks
            var taskRecords = _repository.Fetch(x => x.CreatedUtc >= lastIndexing)
                .ToArray();

            if (taskRecords.Length == 0)
                return;

            Logger.Information("Processing {0} indexing tasks", taskRecords.Length);

            if (!_indexProvider.Exists(SearchIndexName)) {
                _indexProvider.CreateIndex(SearchIndexName);
            }

            var updateIndexDocuments = new List<IIndexDocument>();

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

                try {
                    var context = new IndexContentContext {
                        ContentItem = task.ContentItem,
                        IndexDocument = _indexProvider.New(task.ContentItem.Id)
                    };

                    // dispatch to handlers to retrieve index information
                    foreach (var handler in _handlers) {
                        handler.Indexing(context);
                    }

                    updateIndexDocuments.Add(context.IndexDocument);

                    foreach (var handler in _handlers) {
                        handler.Indexed(context);
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
    }
}
