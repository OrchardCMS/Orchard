using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Search.Models;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Orchard.Collections;
using Orchard.Themes;

namespace Orchard.Search.Controllers {
    [ValidateInput(false), Themed]
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;

        public SearchController(
            IOrchardServices services,
            ISearchService searchService, 
            IContentManager contentManager) {

             Services = services;
            _searchService = searchService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string q, int page = 1, int pageSize = 10) {
            var searchFields = Services.WorkContext.CurrentSite.As<SearchSettingsPart>().SearchedFields;

            IPageOfItems<ISearchHit> searchHits;
            
            if (q.Trim().StartsWith("?") || q.Trim().StartsWith("*")) {
                searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
                Services.Notifier.Error(T("'*' or '?' not allowed as first character in WildcardQuery"));
            } 
            else {
                searchHits = _searchService.Query(q, page, pageSize,
                                                      Services.WorkContext.CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                                                      searchFields,
                                                      searchHit => searchHit);
            }

            var searchResultViewModels = new List<SearchResultViewModel>();

            foreach(var searchHit in searchHits) {
                var contentItem = _contentManager.Get(searchHit.ContentItemId);
                // ignore search results which content item has been removed or unpublished
                if(contentItem == null){
                    searchHits.TotalItemCount--;
                    continue;
                }

                searchResultViewModels.Add(new SearchResultViewModel {
                        Content = _contentManager.BuildDisplay(contentItem, "SummaryForSearch"),
                        SearchHit = searchHit
                    });
            }

            var pageOfItems = new PageOfItems<SearchResultViewModel>(searchResultViewModels) {
                PageNumber = page,
                PageSize = searchHits.PageSize,
                TotalItemCount = searchHits.TotalItemCount
            };

            var searchViewModel = new SearchViewModel {
                Query = q,
                DefaultPageSize = 10, // TODO: sebastien <- yeah, I know :|
                PageOfResults = pageOfItems
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }
    }
}