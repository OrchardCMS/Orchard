using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Controllers {
    public class Routable : ContentPartDriver<RoutableAspect> {
        private const string TemplateName = "Parts/Common.Routable";

        protected override string Prefix {
            get { return "Routable"; }
        }

        protected override DriverResult Editor(RoutableAspect part) {
            var model = new RoutableEditorViewModel { Prefix = Prefix, RoutableAspect = part };
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "before.5");
        }

        protected override DriverResult Editor(RoutableAspect part, IUpdateModel updater) {
            var model = new RoutableEditorViewModel { Prefix = Prefix, RoutableAspect = part };
            updater.TryUpdateModel(model, Prefix, null, null);
            return ContentPartTemplate(model, TemplateName, Prefix).Location("primary", "before.5");
        }
    }
}