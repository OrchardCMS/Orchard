using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Indexing.Services
{
    public class IndexingService : IIndexingService {
        private readonly IIndexManager _indexManager;
        private readonly IEnumerable<IIndexNotifierHandler> _indexNotifierHandlers;
        private readonly IIndexStatisticsProvider _indexStatisticsProvider;

        public IndexingService(
            IOrchardServices services, 
            IIndexManager indexManager, 
            IEnumerable<IIndexNotifierHandler> indexNotifierHandlers,
            IIndexStatisticsProvider indexStatisticsProvider) {
            Services = services;
            _indexManager = indexManager;
            _indexNotifierHandlers = indexNotifierHandlers;
            _indexStatisticsProvider = indexStatisticsProvider;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        void IIndexingService.RebuildIndex(string indexName) {
            if (!_indexManager.HasIndexProvider()) {
                Services.Notifier.Warning(T("There is no search index to rebuild."));
                return;
            }

            var searchProvider = _indexManager.GetSearchIndexProvider();
            if (searchProvider.Exists(indexName))
                searchProvider.DeleteIndex(indexName);

            searchProvider.CreateIndex(indexName); // or just reset the updated date and let the background process recreate the index

            Services.Notifier.Information(T("The index {0} has been rebuilt.", indexName));
        }

        void IIndexingService.UpdateIndex(string indexName) {
            
            foreach(var handler in _indexNotifierHandlers) {
                handler.UpdateIndex(indexName);
            }

            Services.Notifier.Information(T("The search index has been updated."));
        }

        IndexEntry IIndexingService.GetIndexEntry(string indexName) {
            var provider = _indexManager.GetSearchIndexProvider();
            if (provider == null)
                return null;

            return new IndexEntry {
                IndexName = indexName,
                DocumentCount = provider.NumDocs(indexName),
                Fields = provider.GetFields(indexName),
                LastUpdateUtc = _indexStatisticsProvider.GetLastIndexedUtc(indexName),
                IndexingStatus = _indexStatisticsProvider.GetIndexingStatus(indexName)
            };
        }
    }
}