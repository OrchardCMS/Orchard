using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Core.Routable.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Routable.Drivers {
    public class RoutableDriver : ContentPartDriver<IsRoutable> {
        private readonly IOrchardServices _services;
        private readonly IRoutableService _routableService;

        public RoutableDriver(IOrchardServices services, IRoutableService routableService) {
            _services = services;
            _routableService = routableService;
            T = NullLocalizer.Instance;
        }

        private const string TemplateName = "Parts/Routable.IsRoutable";

        public Localizer T { get; set; }

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

        string GetContainerSlug(IContent item) {
            var commonAspect = item.As<ICommonPart>();
            if (commonAspect != null && commonAspect.Container != null) {
                var routable = commonAspect.Container.As<IRoutableAspect>();
                if (routable != null) {
                    return routable.Slug;
                }
            }
            return null;
        }

        protected override DriverResult Editor(IsRoutable part) {
            var model = new RoutableEditorViewModel {
                ContentType = part.ContentItem.ContentType,
                Id = part.ContentItem.Id,
                Slug = part.Slug,
                Title = part.Title,
                ContainerId = GetContainerId(part),
            };

            // TEMP: path format patterns replaces this logic
            var path = part.Path;
            if (path != null && path.EndsWith(part.Slug)) {
                model.DisplayLeadingPath = path.Substring(0, path.Length - part.Slug.Length);
            }
            else {
                var containerSlug = GetContainerSlug(part);
                model.DisplayLeadingPath = !string.IsNullOrWhiteSpace(containerSlug)
                    ? string.Format("{0}/", containerSlug)
                    : "";
            }

            var location = part.GetLocation("Editor");
            return ContentPartTemplate(model, TemplateName, Prefix).Location(location);
        }

        protected override DriverResult Editor(IsRoutable part, IUpdateModel updater) {

            var model = new RoutableEditorViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);
            part.Title = model.Title;
            part.Slug = model.Slug;

            // TEMP: path format patterns replaces this logic
            var containerSlug = GetContainerSlug(part);
            if (string.IsNullOrEmpty(containerSlug)) {
                part.Path = model.Slug;
            }
            else {
                part.Path = containerSlug + "/" + model.Slug;
            }

            if (!_routableService.IsSlugValid(part.Slug)) {
                updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \"/\", \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead)."));
            }

            string originalSlug = part.Slug;
            if (!_routableService.ProcessSlug(part)) {
                _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                    originalSlug, part.Slug, part.ContentItem.ContentType));
            }

            return Editor(part);
        }

    }
}