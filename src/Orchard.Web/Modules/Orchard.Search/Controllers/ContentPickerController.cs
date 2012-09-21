using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Search.Models;
using Orchard.Search.Services;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {
    [Admin]
    [OrchardFeature("Orchard.Search.ContentPicker")]
    public class ContentPickerController : Controller {
        private readonly ISearchService _searchService;
        private readonly ISiteService _siteService;

        public ContentPickerController(
            IOrchardServices orchardServices,
            ISearchService searchService,
            ISiteService siteService) {
            _searchService = searchService;
            _siteService = siteService;
            Services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [Themed(false)]
        public ActionResult Index(PagerParameters pagerParameters, string searchText = "") {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchFields = Services.WorkContext.CurrentSite.As<SearchSettingsPart>().SearchedFields;

            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try {

                searchHits = _searchService.Query(searchText, pager.Page, pager.PageSize,
                                                  Services.WorkContext.CurrentSite.As<SearchSettingsPart>().Record.FilterCulture,
                                                  searchFields,
                                                  searchHit => searchHit);
            }
            catch (Exception exception) {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Invalid search query: {0}", exception.Message));
            }

            var list = Services.New.List();
            foreach (var contentItem in Services.ContentManager.GetMany<IContent>(searchHits.Select(x => x.ContentItemId), VersionOptions.Published, QueryHints.Empty)) {
                // ignore search results which content item has been removed or unpublished
                if (contentItem == null) {
                    searchHits.TotalItemCount--;
                    continue;
                }

                list.Add(Services.ContentManager.BuildDisplay(contentItem, "SummaryAdmin"));
            }

            var pagerShape = Services.New.Pager(pager).TotalItemCount(searchHits.TotalItemCount);


            foreach(IShape item in list.Items) {
                item.Metadata.Type = "ContentPicker";
            }

            // retain the parameter in the pager links
            RouteData.Values["searchText"] = searchText;

            dynamic tab = Services.New.SearchContentTab()
                .ContentItems(list)
                .Pager(pagerShape)
                .SearchText(searchText);

            return new ShapeResult(this, Services.New.ContentPicker().Tab(tab));
        }
    }
}