using System.Collections.Generic;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Validation;

namespace Orchard.Indexing.Services {
    public class IndexingService : IIndexingService {
        private readonly IIndexManager _indexManager;
        private readonly IEnumerable<IIndexNotifierHandler> _indexNotifierHandlers;
        private readonly IIndexStatisticsProvider _indexStatisticsProvider;
        private readonly IIndexingTaskExecutor _indexingTaskExecutor;

        public IndexingService(
            IOrchardServices services, 
            IIndexManager indexManager, 
            IEnumerable<IIndexNotifierHandler> indexNotifierHandlers,
            IIndexStatisticsProvider indexStatisticsProvider,
            IIndexingTaskExecutor indexingTaskExecutor) {
            Services = services;
            _indexManager = indexManager;
            _indexNotifierHandlers = indexNotifierHandlers;
            _indexStatisticsProvider = indexStatisticsProvider;
            _indexingTaskExecutor = indexingTaskExecutor;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public void RebuildIndex(string indexName) {
            Argument.ThrowIfNullOrEmpty(indexName, "indexName");

            if (!_indexManager.HasIndexProvider()) {
                Services.Notifier.Warning(T("There is no index to rebuild."));
                return;
            }

            if(_indexingTaskExecutor.RebuildIndex(indexName)) {
                Services.Notifier.Information(T("The index {0} has been rebuilt.", indexName));
                UpdateIndex(indexName);
            }
            else {
                Services.Notifier.Warning(T("The index {0} could not be rebuilt. It might already be in use, please try again later.", indexName));
            }
        }

        public void DeleteIndex(string indexName) {
            Argument.ThrowIfNullOrEmpty(indexName, "indexName");

            if (!_indexManager.HasIndexProvider()) {
                Services.Notifier.Warning(T("There is no index to delete."));
                return;
            }

            if (_indexingTaskExecutor.DeleteIndex(indexName)) {
                Services.Notifier.Information(T("The index {0} has been deleted.", indexName));
            }
            else {
                Services.Notifier.Warning(T("The index {0} could not be deleted. It might already be in use, please try again later.", indexName));
            }
        }

        public void UpdateIndex(string indexName) {
            Argument.ThrowIfNullOrEmpty(indexName, "indexName");
            
            foreach(var handler in _indexNotifierHandlers) {
                handler.UpdateIndex(indexName);
            }

            Services.Notifier.Information(T("The index {0} has been updated.", indexName));
        }

        IndexEntry IIndexingService.GetIndexEntry(string indexName) {
            Argument.ThrowIfNullOrEmpty(indexName, "indexName");

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