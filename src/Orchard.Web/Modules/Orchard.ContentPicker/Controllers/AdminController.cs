using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Orchard.ContentPicker.Controllers {
    public class AdminController : Controller {
        private readonly ISiteService _siteService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdminController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IContentDefinitionManager contentDefinitionManager) {
            _siteService = siteService;
            _contentDefinitionManager = contentDefinitionManager;
            Services = orchardServices;
        }

        public IOrchardServices Services { get; set; }

        [Themed(false)]
        public ActionResult Index(ListContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var query = Services.ContentManager.Query(VersionOptions.Latest, GetCreatableTypes(false).Select(ctd => ctd.Name).ToArray());

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

            model.Options.FilterOptions = GetCreatableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            var pagerShape = Services.New.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Services.New.List();
            list.AddRange(pageOfContentItems.Select(ci => Services.ContentManager.BuildDisplay(ci, "SummaryAdmin")));

            foreach(IShape item in list.Items) {
                item.Metadata.Type = "ContentPicker";
            }

            dynamic tab = Services.New.RecentContentTab()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            // retain the parameter in the pager links
            RouteData.Values["Options.SelectedFilter"] = model.Options.SelectedFilter;
            RouteData.Values["Options.OrderBy"] = model.Options.OrderBy.ToString();

            return new ShapeResult(this, Services.New.ContentPicker().Tab(tab));
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}