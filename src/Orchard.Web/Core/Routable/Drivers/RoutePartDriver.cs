using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Core.Routable.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Routable.Drivers {
    public class RoutePartDriver : ContentPartDriver<RoutePart> {
        private readonly IOrchardServices _services;
        private readonly IRoutableService _routableService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHomePageProvider _routableHomePageProvider;

        public RoutePartDriver(IOrchardServices services,
            IRoutableService routableService,
            IEnumerable<IHomePageProvider> homePageProviders,
            IHttpContextAccessor httpContextAccessor) {
            _services = services;
            _routableService = routableService;
            _httpContextAccessor = httpContextAccessor;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            T = NullLocalizer.Instance;
        }

        private const string TemplateName = "Parts.Routable.RoutePart";

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Routable"; }
        }

        static int? GetContainerId(IContent item) {
            var commonPart = item.As<ICommonPart>();
            if (commonPart != null && commonPart.Container != null) {
                return commonPart.Container.ContentItem.Id;
            }
            return null;
        }

        protected override DriverResult Display(RoutePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_RoutableTitle",
                    () => shapeHelper.Parts_RoutableTitle(ContentPart: part, Title: part.Title, Path: part.Path)),
                ContentShape("Parts_RoutableTitle_Summary",
                    () => shapeHelper.Parts_RoutableTitle_Summary(ContentPart: part, Title: part.Title, Path: part.Path)),
                ContentShape("Parts_RoutableTitle_SummaryAdmin",
                    () => shapeHelper.Parts_RoutableTitle_SummaryAdmin(ContentPart: part, Title: part.Title, Path: part.Path))
                );
        }

        protected override DriverResult Editor(RoutePart part, dynamic shapeHelper) {
            var model = new RoutableEditorViewModel {
                ContentType = part.ContentItem.ContentType,
                Id = part.ContentItem.Id,
                Slug = part.Slug,
                Title = part.Title,
                ContainerId = GetContainerId(part),
            };

            var request = _httpContextAccessor.Current().Request;
            var containerUrl = new UriBuilder(request.ToRootUrlString()) { Path = (request.ApplicationPath ?? "").TrimEnd('/') + "/" + (part.GetContainerPath() ?? "") };
            model.ContainerAbsoluteUrl = containerUrl.Uri.ToString().TrimEnd('/');

            model.PromoteToHomePage = model.Id != 0 && _routableHomePageProvider != null && _services.WorkContext.CurrentSite.HomePage == _routableHomePageProvider.GetSettingValue(model.Id);
            return ContentShape("Parts_Routable_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(RoutePart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new RoutableEditorViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            part.Title = model.Title;
            part.Slug = model.Slug;
            part.PromoteToHomePage = model.PromoteToHomePage;

            if ( !_routableService.IsSlugValid(part.Slug) ) {
                var slug = (part.Slug ?? String.Empty);
                if ( slug.StartsWith(".") || slug.EndsWith(".") )
                    updater.AddModelError("Routable.Slug", T("The \".\" can't be used at either end of the permalink."));
                else
                    updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \", \"<\", \">\", \"\\\". No spaces are allowed (please use dashes or underscores instead)."));
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(RoutePart part, ImportContentContext context) {
            var title = context.Attribute(part.PartDefinition.Name, "Title");
            if (title != null) {
                part.Title = title;
            }

            var slug = context.Attribute(part.PartDefinition.Name, "Slug");
            if (slug != null) {
                part.Slug = slug;
            }

            var path = context.Attribute(part.PartDefinition.Name, "Path");
            if (path != null) {
                part.Path = path;
            }

            var promoteToHomePage = context.Attribute(part.PartDefinition.Name, "PromoteToHomePage");
            if (promoteToHomePage != null) {
                part.PromoteToHomePage = Convert.ToBoolean(promoteToHomePage);
                if (part.PromoteToHomePage && _routableHomePageProvider != null) {
                    _services.WorkContext.CurrentSite.HomePage = _routableHomePageProvider.GetSettingValue(part.ContentItem.Id);
                }
            }
        }

        protected override void Exporting(RoutePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Title", part.Title);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Slug", part.Slug);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Path", part.Path);
            if (_services.WorkContext.CurrentSite.HomePage == _routableHomePageProvider.GetSettingValue(part.ContentItem.Id)) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("PromoteToHomePage", "true");   
            }
        }
    }
}