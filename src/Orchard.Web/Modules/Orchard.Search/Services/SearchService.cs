using System;
using System.Globalization;
using System.Linq;
using Orchard.Collections;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;

namespace Orchard.Search.Services {
    public class SearchService : ISearchService {
        private readonly IIndexManager _indexManager;
        private readonly ICultureManager _cultureManager;

        public SearchService(IOrchardServices services, IIndexManager indexManager, ICultureManager cultureManager) {
            Services = services;
            _indexManager = indexManager;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        ISearchBuilder Search(string index) {
            return _indexManager.HasIndexProvider()
                ? _indexManager.GetSearchIndexProvider().CreateSearchBuilder(index)
                : new NullSearchBuilder();
        }

        IPageOfItems<T> ISearchService.Query<T>(string query, int page, int? pageSize, bool filterCulture, string index, string[] searchFields, Func<ISearchHit, T> shapeResult) {

            if (string.IsNullOrWhiteSpace(query))
                return new PageOfItems<T>(Enumerable.Empty<T>());

            var searchBuilder = Search(index).Parse(searchFields, query);

            if (filterCulture) {
                var culture = _cultureManager.GetCurrentCulture(Services.WorkContext.HttpContext);

                // use LCID as the text representation gets analyzed by the query parser
                searchBuilder
                    .WithField("culture", CultureInfo.GetCultureInfo(culture).LCID)
                    .AsFilter();
            }

            var totalCount = searchBuilder.Count();
            if (pageSize != null)
                searchBuilder = searchBuilder
                    .Slice((page > 0 ? page - 1 : 0) * (int)pageSize, (int)pageSize);

            var searchResults = searchBuilder.Search();

            var pageOfItems = new PageOfItems<T>(searchResults.Select(shapeResult)) {
                PageNumber = page,
                PageSize = pageSize != null ? (int)pageSize : totalCount,
                TotalItemCount = totalCount
            };

            return pageOfItems;
        }
    }
}