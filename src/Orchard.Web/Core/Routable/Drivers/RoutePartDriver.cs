using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Core.Routable.ViewModels;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Core.Routable.Drivers {
    public class RoutePartDriver : ContentPartDriver<RoutePart> {
        private readonly IOrchardServices _services;
        private readonly IRoutableService _routableService;
        private readonly IHomePageProvider _routableHomePageProvider;

        public RoutePartDriver(IOrchardServices services, IRoutableService routableService, IEnumerable<IHomePageProvider> homePageProviders) {
            _services = services;
            _routableService = routableService;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name); ;
            T = NullLocalizer.Instance;
        }

        private const string TemplateName = "Parts/Routable.RoutePart";

        public Localizer T { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        protected override string Prefix {
            get { return "Routable"; }
        }

        int? GetContainerId(IContent item) {
            var commonPart = item.As<ICommonPart>();
            if (commonPart != null && commonPart.Container != null) {
                return commonPart.Container.ContentItem.Id;
            }
            return null;
        }

        protected override DriverResult Editor(RoutePart part) {
            var model = new RoutableEditorViewModel {
                ContentType = part.ContentItem.ContentType,
                Id = part.ContentItem.Id,
                Slug = part.Slug,
                Title = part.Title,
                ContainerId = GetContainerId(part),
            };

            // TEMP: path format patterns replaces this logic
            var path = part.Path;
            var slug = part.Slug ?? "";
            if (path != null && path.EndsWith(slug)) {
                model.DisplayLeadingPath = path.Substring(0, path.Length - slug.Length);
            }
            else {
                var containerSlug = part.GetContainerSlug();
                model.DisplayLeadingPath = !string.IsNullOrWhiteSpace(containerSlug)
                    ? string.Format("{0}/", containerSlug)
                    : "";
            }

            var location = part.GetLocation("Editor");
            model.PromoteToHomePage = model.Id != 0 && part.Path != null && _routableHomePageProvider != null && CurrentSite.HomePage == _routableHomePageProvider.GetSettingValue(model.Id);
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        protected override DriverResult Editor(RoutePart part, IUpdateModel updater) {

            var model = new RoutableEditorViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);
            part.Title = model.Title;
            part.Slug = model.Slug;
            part.Path = part.GetPathFromSlug(model.Slug);

            if (!_routableService.IsSlugValid(part.Slug)) {
                updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \"/\", \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead)."));
            }

            string originalSlug = part.Slug;
            if (!_routableService.ProcessSlug(part)) {
                _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                    originalSlug, part.Slug, part.ContentItem.ContentType));
            }

            // TEMP: path format patterns replaces this logic
            part.Path = part.GetPathFromSlug(part.Slug);

            if (part.ContentItem.Id != 0 && model.PromoteToHomePage && _routableHomePageProvider != null) {
                CurrentSite.HomePage = _routableHomePageProvider.GetSettingValue(part.ContentItem.Id);
            }

            return Editor(part);
        }
    }
}