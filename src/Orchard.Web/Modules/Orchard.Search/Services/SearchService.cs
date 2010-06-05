using System;
using System.Collections.Generic;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Models;
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

        ISearchResult ISearchService.Query(string query, int skip, int? take) {
            if (string.IsNullOrWhiteSpace(query) || !_indexManager.HasIndexProvider())
                return null;

            var searchBuilder =  _indexManager.GetSearchIndexProvider().CreateSearchBuilder(SearchIndexName)
                .WithField("title", query)
                .WithField("body", query);

            var totalCount = searchBuilder.Count();
            if (take != null)
                searchBuilder = searchBuilder
                    .Slice(skip, (int)take);

            return new SearchResult {
                Page = searchBuilder.Search(),
                TotalCount = totalCount
            };
        }
        
        void ISearchService.RebuildIndex() {
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

        void ISearchService.UpdateIndex() {
            
            foreach(var handler in _indexNotifierHandlers) {
                handler.UpdateIndex(SearchIndexName);
            }

            Services.Notifier.Information(T("The search index has been updated."));
        }

        DateTime ISearchService.GetIndexUpdatedUtc() {
            return !HasIndexToManage
                ? DateTime.MinValue
                : _indexManager.GetSearchIndexProvider().GetLastIndexUtc(SearchIndexName);
        }
    }
}