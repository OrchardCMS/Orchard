using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Core.Routable.ViewModels;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Notify;

namespace Orchard.Core.Routable.Drivers {
    public class RoutePartDriver : ContentPartDriver<RoutePart> {
        private readonly IOrchardServices _services;
        private readonly IRoutableService _routableService;
        private readonly IHomePageProvider _routableHomePageProvider;

        public RoutePartDriver(IOrchardServices services, IRoutableService routableService, IEnumerable<IHomePageProvider> homePageProviders) {
            _services = services;
            _routableService = routableService;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            T = NullLocalizer.Instance;
        }

        private const string TemplateName = "Parts/Routable.RoutePart";

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
            return ContentShape("Parts_RoutableTitle", 
                () => shapeHelper.Parts_RoutableTitle(ContentPart: part, Title: part.Title, Path: part.Path));
        }

        protected override DriverResult Editor(RoutePart part, dynamic shapeHelper) {
            var model = new RoutableEditorViewModel {
                ContentType = part.ContentItem.ContentType,
                Id = part.ContentItem.Id,
                Slug = part.GetEffectiveSlug(),
                Title = part.Title,
                ContainerId = GetContainerId(part),
            };

            var containerPath = part.GetContainerPath();
            model.DisplayLeadingPath = !string.IsNullOrWhiteSpace(containerPath)
                ? string.Format("{0}/", containerPath)
                : "";

            model.PromoteToHomePage = model.Id != 0 && part.Path != null && _routableHomePageProvider != null && _services.WorkContext.CurrentSite.HomePage == _routableHomePageProvider.GetSettingValue(model.Id);
            return ContentShape("Parts_Routable_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(RoutePart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new RoutableEditorViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            part.Title = model.Title;
            part.Slug = model.Slug;

            if ( !_routableService.IsSlugValid(part.Slug) ) {
                var slug = (part.Slug ?? String.Empty);
                if ( slug.StartsWith(".") || slug.EndsWith(".") )
                    updater.AddModelError("Routable.Slug", T("The \".\" can't be used around routes."));
                else
                    updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead)."));
            }

            if (part.ContentItem.Id != 0 && model.PromoteToHomePage && _routableHomePageProvider != null)
                _services.WorkContext.CurrentSite.HomePage = _routableHomePageProvider.GetSettingValue(part.ContentItem.Id);

            return Editor(part, shapeHelper);
        }
    }
}