using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Search.Services
{
    public class SearchService : ISearchService
    {
        private const string SearchIndexName = "Search";
        private readonly IIndexManager _indexManager;
        private readonly IEnumerable<IIndexNotifierHandler> _indexNotifierHandlers;

        public SearchService(IOrchardServices services, IIndexManager indexManager, IEnumerable<IIndexNotifierHandler> indexNotifierHandlers) {
            Services = services;
            _indexManager = indexManager;
            _indexNotifierHandlers = indexNotifierHandlers;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public bool HasIndexToManage {
            get { return _indexManager.HasIndexProvider(); }
        }

        public IEnumerable<ISearchHit> Query(string term) {
            if (string.IsNullOrWhiteSpace(term) || !_indexManager.HasIndexProvider())
                return Enumerable.Empty<ISearchHit>();

            return _indexManager.GetSearchIndexProvider().CreateSearchBuilder(SearchIndexName)
                .WithField("title", term)
                .WithField("body", term)
                .Search();
        }
        
        public void RebuildIndex() {
            if (!_indexManager.HasIndexProvider()) {
                Services.Notifier.Warning(T("There is no search index to rebuild."));
                return;
            }

            var searchProvider = _indexManager.GetSearchIndexProvider();
            if (searchProvider.Exists(SearchIndexName))
                searchProvider.DeleteIndex(SearchIndexName);

            searchProvider.CreateIndex(SearchIndexName); // or just reset the updated date and let the background process recreate the index
            Services.Notifier.Information(T("The search index has been rebuilt."));
        }

        public void UpdateIndex() {
            
            foreach(var handler in _indexNotifierHandlers) {
                handler.UpdateIndex(SearchIndexName);
            }

            Services.Notifier.Information(T("The search index has been updated."));
        }

        public DateTime GetIndexUpdatedUtc() {
            if(!HasIndexToManage) {
                return DateTime.MinValue;
            }

            return _indexManager.GetSearchIndexProvider().GetLastIndexUtc(SearchIndexName);
        }
    }
}