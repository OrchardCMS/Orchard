using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.ViewModels;

namespace Orchard.Core.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizationDriver : ContentPartDriver<Localized> {
        public LocalizationDriver(IOrchardServices services) {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(Localized part, string displayType) {
            if (!Services.Authorizer.Authorize(Permissions.ChangeOwner)) {
                return null;
            }

            var model = new ContentTranslationsViewModel(part);
            return ContentPartTemplate(model, "Parts/Localization.ContentTranslations").LongestMatch(displayType, "Summary", "SummaryAdmin").Location("primary", "5");
        }

        protected override DriverResult Editor(Localized part) {
            var model = new LocalizationEditorViewModel();
            //if (part.ContentItem.Is<Localized>())

            return ContentPartTemplate(model, "Parts/Localization.IsLocalized").Location("primary", "before.3");
        }
    }

    public class LocalizationEditorViewModel {}
}