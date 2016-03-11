using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Search.Helpers;
using Orchard.Search.Models;
using Orchard.Search.Settings;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {
    [Admin]
    [OrchardFeature("Orchard.Search.ContentPicker")]
    public class ContentPickerController : Controller {
        private readonly ISiteService _siteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IIndexManager _indexManager;

        public ContentPickerController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IContentDefinitionManager contentDefinitionManager,
            IIndexManager indexManager) {
            _siteService = siteService;
            _contentDefinitionManager = contentDefinitionManager;
            _indexManager = indexManager;
            Services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [Themed(false)]
        public ActionResult Index(PagerParameters pagerParameters, string part, string field, string searchText = "") {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchSettingsPart = Services.WorkContext.CurrentSite.As<SearchSettingsPart>();
            var totalCount = 0;
            var foundIds = new int[0];

            if (!String.IsNullOrWhiteSpace(searchText)) {
                ContentPickerSearchFieldSettings settings = null;
                // if the picker is loaded for a specific field, apply custom settings
                if (!String.IsNullOrEmpty(part) && !String.IsNullOrEmpty(field)) {
                    var definition = _contentDefinitionManager.GetPartDefinition(part).Fields.FirstOrDefault(x => x.Name == field);
                    if (definition != null) {
                        settings = definition.Settings.GetModel<ContentPickerSearchFieldSettings>();
                    }
                }

                if (!_indexManager.HasIndexProvider()) {
                    return View("NoIndex");
                }

                var searchIndex = searchSettingsPart.SearchIndex;
                if (settings != null && !String.IsNullOrEmpty(settings.SearchIndex))
                    searchIndex = settings.SearchIndex;
                var searchFields = searchSettingsPart.GetSearchFields(searchIndex);

                var builder = _indexManager.GetSearchIndexProvider().CreateSearchBuilder(searchIndex);

                try {
                    builder.Parse(searchFields, searchText);

                    if (settings != null && !String.IsNullOrEmpty(settings.DisplayedContentTypes)) {
                        var rawTypes = settings.DisplayedContentTypes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        var contentTypes = _contentDefinitionManager
                            .ListTypeDefinitions()
                            .Where(x => x.Parts.Any(p => rawTypes.Contains(p.PartDefinition.Name)) || rawTypes.Contains(x.Name))
                            .ToArray();


                        foreach (string type in contentTypes.Select(x => x.Name)) {
                            builder.WithField("type", type).NotAnalyzed().AsFilter();
                        }
                    }

                    totalCount = builder.Count();
                    builder = builder.Slice((pager.Page > 0 ? pager.Page - 1 : 0) * pager.PageSize, pager.PageSize);
                    var searchResults = builder.Search();

                    foundIds = searchResults.Select(searchHit => searchHit.ContentItemId).ToArray();
                }
                catch (Exception exception) {
                    Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                    Services.Notifier.Error(T("Invalid search query: {0}", exception.Message));
                }
            }

            var list = Services.New.List();
            foreach (var contentItem in Services.ContentManager.GetMany<IContent>(foundIds, VersionOptions.Published, QueryHints.Empty)) {
                // ignore search results which content item has been removed or unpublished
                if (contentItem == null) {
                    totalCount--;
                    continue;
                }

                list.Add(Services.ContentManager.BuildDisplay(contentItem, "SummaryAdmin"));
            }

            var pagerShape = Services.New.Pager(pager).TotalItemCount(totalCount);

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
