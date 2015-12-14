using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Search.Helpers;
using Orchard.Search.Models;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {

    [ValidateInput(false), Themed]
    public class SearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public SearchController(
            IOrchardServices services,
            ISearchService searchService,
            IContentManager contentManager,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
             Services = services;
            _searchService = searchService;
            _contentManager = contentManager;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(PagerParameters pagerParameters, string q = "") {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchSettingPart = Services.WorkContext.CurrentSite.As<SearchSettingsPart>();

            if (String.IsNullOrEmpty(searchSettingPart.SearchIndex)) {
                Services.Notifier.Error(T("Please define a default search index"));
                return HttpNotFound();
            }

            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try {

                searchHits = _searchService.Query(q, pager.Page, pager.PageSize,
                                                  Services.WorkContext.CurrentSite.As<SearchSettingsPart>().FilterCulture,
                                                  searchSettingPart.SearchIndex,
                                                  searchSettingPart.GetSearchFields(),
                                                  searchHit => searchHit);
            } catch(Exception exception) {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Invalid search query: {0}", exception.Message));
            }

            var list = Shape.List();
            var foundIds = searchHits.Select(searchHit => searchHit.ContentItemId).ToList();

            // ignore search results which content item has been removed or unpublished
            var foundItems = _contentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints()).ToList();
            foreach (var contentItem in foundItems) {
                list.Add(_contentManager.BuildDisplay(contentItem, searchSettingPart.DisplayType));
            }
            searchHits.TotalItemCount -= foundIds.Count() - foundItems.Count();

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