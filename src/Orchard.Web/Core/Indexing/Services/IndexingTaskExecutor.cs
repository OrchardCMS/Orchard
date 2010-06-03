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
        private readonly IRepository<IndexingSettingsRecord> _settings;
        private readonly IEnumerable<IContentHandler> _handlers;
        private IIndexProvider _indexProvider;
        private IIndexManager _indexManager;
        private readonly IContentManager _contentManager;
        private const string SearchIndexName = "search";

        public IndexingTaskExecutor(
            IClock clock,
            IRepository<IndexingTaskRecord> repository,
            IRepository<IndexingSettingsRecord> settings,
            IEnumerable<IContentHandler> handlers,
            IIndexManager indexManager,
            IContentManager contentManager) {
            _clock = clock;
            _repository = repository;
            _settings = settings;
            _indexManager = indexManager;
            _handlers = handlers;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {

            if(!_indexManager.HasIndexProvider()) {
                return;
            }

            _indexProvider = _indexManager.GetSearchIndexProvider();

            // retrieve last processed index time
            var settingsRecord = _settings.Table.FirstOrDefault();

            if (settingsRecord == null) {
                _settings.Create(settingsRecord = new IndexingSettingsRecord { LatestIndexingUtc = new DateTime(1980, 1, 1)});
            }

            var lastIndexing = settingsRecord.LatestIndexingUtc;
            settingsRecord.LatestIndexingUtc = _clock.UtcNow;

            // retrieved not yet processed tasks
            var taskRecords = _repository.Fetch(x => x.CreatedUtc >= lastIndexing)
                .ToArray();
            
            if (taskRecords.Length == 0)
                return;

            Logger.Information("Processing {0} indexing tasks", taskRecords.Length);

            
            if(!_indexProvider.Exists(SearchIndexName)) {
                _indexProvider.CreateIndex(SearchIndexName);
            }

            foreach (var taskRecord in taskRecords) {

                try {
                    var task = new IndexingTask(_contentManager, taskRecord);
                    var context = new IndexContentContext {
                                                              ContentItem = task.ContentItem,
                                                              IndexDocument = _indexProvider.New(task.ContentItem.Id)
                    };

                    // dispatch to handlers to retrieve index information
                    foreach (var handler in _handlers) {
                        handler.Indexing(context);
                    }

                    _indexProvider.Store(SearchIndexName, context.IndexDocument);

                    foreach ( var handler in _handlers ) {
                        handler.Indexed(context);
                    }
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "Unable to process indexing task #{0}", taskRecord.Id);
                }

            }

            _settings.Update(settingsRecord);
        }
    }
}
