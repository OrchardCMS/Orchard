using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common;
using Orchard.Core.Localization.Models;
using Orchard.Core.Localization.ViewModels;

namespace Orchard.Core.Localization.Drivers {
    [UsedImplicitly]
    public class LocalizedDriver : ContentPartDriver<Localized> {
        public LocalizedDriver(IOrchardServices services) {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(Localized part, string displayType) {
            if (!Services.Authorizer.Authorize(Permissions.ChangeOwner))
                return null;

            var model = new ContentTranslationsViewModel(part);
            return  ContentPartTemplate(model, "Parts/Localized.ContentTranslations").LongestMatch(displayType, "Summary", "SummaryAdmin").Location("primary", "5");
        }
    }
}