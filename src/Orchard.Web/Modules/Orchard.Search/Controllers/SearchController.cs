using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Search.Models;
using Orchard.UI.Navigation;
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
            IContentManager contentManager, 
            IShapeFactory shapeFactory) {
             Services = services;
            _searchService = searchService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(Pager pager, string q = "") {
            var searchFields = Services.WorkContext.CurrentSite.As<SearchSettingsPart>().SearchedFields;

            IPageOfItems<ISearchHit> searchHits;
            if (q.Trim().StartsWith("?") || q.Trim().StartsWith("*")) {
                searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
                Services.Notifier.Error(T("'*' or '?' not allowed as first character in WildcardQuery"));
            } 
            else {
                searchHits = _searchService.Query(q, pager.Page, pager.PageSize,
                                                      Services.WorkContext.CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                                                      searchFields,
                                                      searchHit => searchHit);
            }

            var list = Shape.List();
            foreach (var contentItem in searchHits.Select(searchHit => _contentManager.Get(searchHit.ContentItemId))) {
                // ignore search results which content item has been removed or unpublished
                if(contentItem == null){
                    searchHits.TotalItemCount--;
                    continue;
                }

                list.Add(_contentManager.BuildDisplay(contentItem, "Summary"));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(searchHits.TotalItemCount);

            var searchViewModel = new SearchViewModel {
                Query = q,
                TotalItemCount = searchHits.TotalItemCount,
                StartPosition = (pager.Page - 1) * pager.PageSize + 1,
                EndPosition = pager.Page * pager.PageSize > searchHits.TotalItemCount ? searchHits.TotalItemCount : pager.Page * pager.PageSize,
                ContentItems = list,
                Pager = pagerShape
            };

            //todo: deal with page requests beyond result count

            return View(searchViewModel);
        }
    }
}