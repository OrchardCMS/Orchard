using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Search.Helpers;
using Orchard.Search.Models;
using Orchard.Search.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {
    [OrchardFeature("Orchard.Search.Blogs")]
    [Admin]
    public class BlogSearchController : Controller {
        private readonly ISearchService _searchService;
        private readonly ISiteService _siteService;
        private readonly IIndexManager _indexManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly ICultureManager _cultureManager;
        private readonly INavigationManager _navigationManager;

        public BlogSearchController(
            IOrchardServices orchardServices,
            ISearchService searchService,
            ISiteService siteService,
            IIndexManager indexManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizer authorizer,
            ICultureManager cultureManager,
            INavigationManager navigationManager,
            IShapeFactory shapeFactory) {

            _searchService = searchService;
            _siteService = siteService;
            Services = orchardServices;
            _indexManager = indexManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _cultureManager = cultureManager;
            _navigationManager = navigationManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public dynamic Shape { get; set; }

        public ActionResult Index(int blogId, PagerParameters pagerParameters, string searchText = "") {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchSettingsPart = Services.WorkContext.CurrentSite.As<SearchSettingsPart>();

            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try {
                if (!string.IsNullOrWhiteSpace(searchText)) {
                    var searchableTypes = new List<string>();
                    // add the type to the list of types we will filter for
                    // BlogPost for now but we would add more types in the future (i.e. "Article")
                    searchableTypes.Add("BlogPost");
                    var searchBuilder = _indexManager.HasIndexProvider()
                    ? _indexManager
                        .GetSearchIndexProvider()
                        .CreateSearchBuilder(BlogSearchConstants.ADMIN_BLOGPOSTS_INDEX)
                    : new NullSearchBuilder();

                    searchBuilder
                        .Parse(searchSettingsPart
                            .GetSearchFields(BlogSearchConstants.ADMIN_BLOGPOSTS_INDEX),
                            searchText);

                    // filter by Blog
                    searchBuilder
                        .WithField("container-id", blogId)
                        .Mandatory()
                        .NotAnalyzed()
                        .AsFilter();

                    foreach (var searchableType in searchableTypes) {
                        // filter by type
                        searchBuilder
                            .WithField("type", searchableType)
                            .NotAnalyzed()
                            .AsFilter();
                    }
                    // pagination
                    var totalCount = searchBuilder.Count();
                    if (pager != null) {
                        searchBuilder = searchBuilder
                            .Slice(
                                (pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize,
                                pager.PageSize);
                    }
                    // search
                    var searchResults = searchBuilder.Search();
                    // prepare the shape for the page
                    searchHits = new PageOfItems<ISearchHit>(searchResults.Select(searchHit => searchHit)) {
                        PageNumber = pager != null ? pager.Page : 0,
                        PageSize = pager != null ? (pager.PageSize != 0 ? pager.PageSize : totalCount) : totalCount,
                        TotalItemCount = totalCount
                    };
                }

            }
            catch (Exception exception) {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Invalid search query: {0}", exception.Message));
            }

            var list = Services.New.List();
            foreach (var contentItem in Services.ContentManager.GetMany<IContent>(searchHits.Select(x => x.ContentItemId), VersionOptions.Latest, QueryHints.Empty)) {
                // ignore search results which content item has been removed
                if (contentItem == null) {
                    searchHits.TotalItemCount--;
                    continue;
                }

                list.Add(Services.ContentManager.BuildDisplay(contentItem, "SummaryAdmin"));
            }

            var pagerShape = Services.New.Pager(pager).TotalItemCount(searchHits.TotalItemCount);

            var viewModel = Services.New.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .SearchText(searchText)
                .BlogId(blogId);

            // Adds LocalMenus; 
            var menuItems = _navigationManager.BuildMenu("blogposts-navigation");
            var request = Services.WorkContext.HttpContext.Request;

            // Set the currently selected path
            Stack<MenuItem> selectedPath = NavigationHelper.SetSelectedPath(menuItems, request, request.RequestContext.RouteData);

            // Populate local nav
            dynamic localMenuShape = Shape.LocalMenu().MenuName("local-admin");

            NavigationHelper.PopulateLocalMenu(Shape, localMenuShape, localMenuShape, menuItems);
            Services.WorkContext.Layout.LocalNavigation.Add(localMenuShape);

            return View(viewModel);
        }
    }
}