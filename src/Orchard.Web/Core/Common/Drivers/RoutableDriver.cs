using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Core.Common.Services;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class Routable : ContentPartDriver<RoutableAspect> {
        private const string TemplateName = "Parts/Common.Routable";

        private readonly IOrchardServices _services;
        private readonly IRoutableService _routableService;
        private Localizer T { get; set; }

        protected override string Prefix {
            get { return "Routable"; }
        }

        public Routable(IOrchardServices services, IRoutableService routableService)
        {
            _services = services;
            _routableService = routableService;

            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(RoutableAspect part) {
            var model = new RoutableEditorViewModel { Prefix = Prefix, RoutableAspect = part };
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "before.5");
        }

        protected override DriverResult Editor(RoutableAspect part, IUpdateModel updater) {
            var model = new RoutableEditorViewModel { Prefix = Prefix, RoutableAspect = part };
            updater.TryUpdateModel(model, Prefix, null, null);

            if (!_routableService.IsSlugValid(part.Slug)){
                updater.AddModelError("Routable.Slug", T("Please do not use any of the following characters in your slugs: \"/\", \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\". No spaces are allowed (please use dashes or underscores instead).").ToString());
            }

            string originalSlug = part.Slug;
            if(!_routableService.ProcessSlug(part)) {
                _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                    originalSlug, part.Slug, part.ContentItem.ContentType));
            }
                
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "before.5");
        }

    }
}