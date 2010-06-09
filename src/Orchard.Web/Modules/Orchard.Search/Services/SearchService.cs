using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Collections;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
using System.Web;

namespace Orchard.Search.Services
{
    public class SearchService : ISearchService
    {
        private const string SearchIndexName = "Search";
        private readonly IIndexManager _indexManager;
        private readonly IEnumerable<IIndexNotifierHandler> _indexNotifierHandlers;
        private readonly ICultureManager _cultureManager;

        public SearchService(IOrchardServices services, IIndexManager indexManager, IEnumerable<IIndexNotifierHandler> indexNotifierHandlers, ICultureManager cultureManager) {
            Services = services;
            _indexManager = indexManager;
            _indexNotifierHandlers = indexNotifierHandlers;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public bool HasIndexToManage {
            get { return _indexManager.HasIndexProvider(); }
        }

        IPageOfItems<T> ISearchService.Query<T>(string query, int page, int? pageSize, Func<ISearchHit, T> shapeResult) {
            if (string.IsNullOrWhiteSpace(query) || !_indexManager.HasIndexProvider())
                return null;

            var searchBuilder = _indexManager.GetSearchIndexProvider().CreateSearchBuilder(SearchIndexName)
                .WithField("title", query)
                .WithField("body", query);

                if(HttpContext.Current != null) {
                    searchBuilder.WithField("culture", _cultureManager.GetCurrentCulture(HttpContext.Current));
                }

            var totalCount = searchBuilder.Count();
            if (pageSize != null)
                searchBuilder = searchBuilder
                    .Slice((page > 0 ? page - 1 : 0) * (int)pageSize, (int)pageSize);


            var pageOfItems = new PageOfItems<T>(searchBuilder.Search().Select(shapeResult)) {
                PageNumber = page,
                PageSize = pageSize != null ? (int) pageSize : totalCount,
                TotalItemCount = totalCount
            };

            return pageOfItems;
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