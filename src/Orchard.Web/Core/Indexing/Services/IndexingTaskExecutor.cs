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
        private readonly IIndexManager _indexManager;
        private readonly IContentManager _contentManager;
        private const string SearchIndexName = "Search";

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

            if ( !_indexManager.HasIndexProvider() ) {
                return;
            }

            _indexProvider = _indexManager.GetSearchIndexProvider();

            // retrieve last processed index time
            var settingsRecord = _settings.Table.FirstOrDefault();

            if ( settingsRecord == null ) {
                _settings.Create(settingsRecord = new IndexingSettingsRecord { LatestIndexingUtc = new DateTime(1980, 1, 1) });
            }

            var lastIndexing = settingsRecord.LatestIndexingUtc;
            settingsRecord.LatestIndexingUtc = _clock.UtcNow;

            // retrieved not yet processed tasks
            var taskRecords = _repository.Fetch(x => x.CreatedUtc >= lastIndexing)
                .ToArray();

            if ( taskRecords.Length == 0 )
                return;

            Logger.Information("Processing {0} indexing tasks", taskRecords.Length);


            if ( !_indexProvider.Exists(SearchIndexName) ) {
                _indexProvider.CreateIndex(SearchIndexName);
            }

            var updateIndexDocuments = new List<IIndexDocument>();
            var deleteIndexDocuments = new List<int>();

            // process Delete tasks
            foreach ( var taskRecord in taskRecords.Where(t => t.Action == IndexingTaskRecord.Delete) ) {
                var task = new IndexingTask(_contentManager, taskRecord);
                deleteIndexDocuments.Add(taskRecord.ContentItemRecord.Id);

                try {
                    _repository.Delete(taskRecord);
                }
                catch ( Exception ex ) {
                    Logger.Error(ex, "Could not delete task #{0}", taskRecord.Id);
                }
            }


            try {
                if ( deleteIndexDocuments.Count > 0 ) {
                    _indexProvider.Delete(SearchIndexName, deleteIndexDocuments);
                }
            }
            catch ( Exception ex ) {
                Logger.Warning(ex, "An error occured while remove a document from the index");
            }

            // process Update tasks
            foreach ( var taskRecord in taskRecords.Where(t => t.Action == IndexingTaskRecord.Update) ) {
                var task = new IndexingTask(_contentManager, taskRecord);

                try {
                    var context = new IndexContentContext {
                        ContentItem = task.ContentItem,
                        IndexDocument = _indexProvider.New(task.ContentItem.Id)
                    };

                    // dispatch to handlers to retrieve index information
                    foreach ( var handler in _handlers ) {
                        handler.Indexing(context);
                    }

                    updateIndexDocuments.Add(context.IndexDocument);

                    foreach ( var handler in _handlers ) {
                        handler.Indexed(context);
                    }
                }
                catch ( Exception ex ) {
                    Logger.Warning(ex, "Unable to process indexing task #{0}", taskRecord.Id);
                }
            }

            try {
                if ( updateIndexDocuments.Count > 0 ) {
                    _indexProvider.Store(SearchIndexName, updateIndexDocuments);
                }
            }
            catch ( Exception ex ) {
                Logger.Warning(ex, "An error occured while adding a document to the index");
            }

            _settings.Update(settingsRecord);
        }
    }
}
