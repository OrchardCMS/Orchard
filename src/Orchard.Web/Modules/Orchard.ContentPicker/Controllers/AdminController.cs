using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentPicker.Settings;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.ContentPicker.Controllers {
    public class AdminController : Controller {
        private readonly ISiteService _siteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INavigationManager _navigationManager;
        private readonly ICultureManager _cultureManager;
        private readonly ICultureFilter _cultureFilter;

        public AdminController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IContentDefinitionManager contentDefinitionManager,
            INavigationManager navigationManager,
            ICultureManager cultureManager,
            ICultureFilter cultureFilter) {
            _siteService = siteService;
            _contentDefinitionManager = contentDefinitionManager;
            _navigationManager = navigationManager;
            Services = orchardServices;
            _cultureManager = cultureManager;
            _cultureFilter = cultureFilter;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        [Themed(false)]
        public ActionResult Index(ListContentsViewModel model, PagerParameters pagerParameters, string part, string field, string types) {
            var menuItems = _navigationManager.BuildMenu("content-picker").ToList();
            var contentPickerMenuItem = menuItems.FirstOrDefault();
            if (contentPickerMenuItem == null) {
                return HttpNotFound();
            }

            if (contentPickerMenuItem.Items.All(x => x.Text.TextHint != "Recent Content")) {
                // the default tab should not be displayed, redirect to the next one
                var root = menuItems.FirstOrDefault();
                if (root == null) {
                    return HttpNotFound();
                }

                var firstChild = root.Items.First();
                if (firstChild == null) {
                    return HttpNotFound();
                }

                var routeData = new RouteValueDictionary(firstChild.RouteValues);
                var queryString = Request.QueryString;
                foreach (var key in queryString.AllKeys) {
                    if (!String.IsNullOrEmpty(key)) {
                        routeData[key] = queryString[key];
                    }
                }

                return RedirectToRoute(routeData);
            }

            ContentPickerFieldSettings settings = null;

            // if the picker is loaded for a specific field, apply custom settings
            if (!String.IsNullOrEmpty(part) && !String.IsNullOrEmpty(field)) {
                var definition = _contentDefinitionManager.GetPartDefinition(part).Fields.FirstOrDefault(x => x.Name == field);
                if (definition != null) {
                    settings = definition.Settings.GetModel<ContentPickerFieldSettings>();
                }
            }

            if (settings != null && !String.IsNullOrEmpty(settings.DisplayedContentTypes)) {
                types = settings.DisplayedContentTypes;
            }

            IEnumerable<ContentTypeDefinition> contentTypes;
            if (!String.IsNullOrEmpty(types)) {
                var rawTypes = types.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                contentTypes = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(x => x.Parts.Any(p => rawTypes.Contains(p.PartDefinition.Name)) || rawTypes.Contains(x.Name))
                    .ToArray();
            }
            else {
                contentTypes = GetListableTypes(false).ToList();
            }

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var query = Services.ContentManager.Query(VersionOptions.Latest, contentTypes.Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.Options.SelectedFilter)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.Options.SelectedFilter);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.Options.SelectedFilter);

            }
            
            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture)) {
                query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            }

            model.Options.FilterOptions = contentTypes
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            model.Options.Cultures = _cultureManager.ListCultures();

            var pagerShape = Services.New.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            var list = Services.New.List();

            list.AddRange(pageOfContentItems.Select(ci => Services.ContentManager.BuildDisplay(ci, "SummaryAdmin")));

            foreach(IShape item in list.Items) {
                item.Metadata.Type = "ContentPicker";
            }

            var tab = Services.New.RecentContentTab()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            // retain the parameter in the pager links
            RouteData.Values["Options.SelectedFilter"] = model.Options.SelectedFilter;
            RouteData.Values["Options.OrderBy"] = model.Options.OrderBy.ToString();
            RouteData.Values["Options.ContentsStatus"] = model.Options.ContentsStatus.ToString();

            return new ShapeResult(this, Services.New.ContentPicker().Tab(tab));
        }

        private IEnumerable<ContentTypeDefinition> GetListableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Listable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}