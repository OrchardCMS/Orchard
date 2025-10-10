using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
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
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {
    [OrchardFeature("Orchard.Search.Content")]
    public class AdminController : Controller {
        private readonly ISearchService _searchService;
        private readonly ISiteService _siteService;
        private readonly IIndexManager _indexManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly ICultureManager _cultureManager;

        public AdminController(
            IOrchardServices orchardServices,
            ISearchService searchService,
            ISiteService siteService,
            IIndexManager indexManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizer authorizer,
            ICultureManager cultureManager) {

            _searchService = searchService;
            _siteService = siteService;
            Services = orchardServices;
            _indexManager = indexManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _cultureManager = cultureManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(PagerParameters pagerParameters, string searchText = "") {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var adminSearchSettingsPart = Services.WorkContext.CurrentSite.As<AdminSearchSettingsPart>();
            var searchSettingsPart = Services.WorkContext.CurrentSite.As<SearchSettingsPart>();
            
            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try {
                // replicate a logic similar to ContentPickerController, but here
                // we want to filter results based on authorized types. This is also
                // partially replicates the logic in SearchService.Search.
                if (!string.IsNullOrWhiteSpace(searchText)) {
                    // select types
                    var contentTypeDefinitions = _contentDefinitionManager
                        .ListTypeDefinitions()
                        .OrderBy(d => d.Name);
                    var listableContentTypes = contentTypeDefinitions
                        .Where(ctd => ctd
                            .Settings
                            .GetModel<ContentTypeSettings>()
                            .Listable);
                    ContentItem listableCi = null;
                    var searchableTypes = new List<string>();
                    foreach (var contentTypeDefinition in listableContentTypes) {
                        listableCi = _contentManager.New(contentTypeDefinition.Name);
                        if (_authorizer.Authorize(Permissions.EditContent, listableCi)) {
                            // add the type to the list of types we will filter for
                            searchableTypes.Add(contentTypeDefinition.Name);
                        }
                    }
                    // we don't even search if no type is allowed
                    if (searchableTypes.Any()) {
                        var searchBuilder = _indexManager.HasIndexProvider()
                        ? _indexManager
                            .GetSearchIndexProvider()
                            .CreateSearchBuilder(adminSearchSettingsPart.SearchIndex)
                        : new NullSearchBuilder();

                        searchBuilder
                            .Parse(searchSettingsPart
                                .GetSearchFields(adminSearchSettingsPart.SearchIndex),
                                searchText);

                        foreach (var searchableType in searchableTypes) {
                            // filter by type
                            searchBuilder
                                .WithField("type", searchableType)
                                .NotAnalyzed()
                                .AsFilter();
                        }
                        // filter by culture?
                        if (searchSettingsPart.FilterCulture) {
                            var culture = _cultureManager.GetCurrentCulture(Services.WorkContext.HttpContext);

                            // use LCID as the text representation gets analyzed by the query parser
                            searchBuilder
                                .WithField("culture", CultureInfo.GetCultureInfo(culture).LCID)
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
                .SearchText(searchText);

            return View(viewModel);
        }
    }
}