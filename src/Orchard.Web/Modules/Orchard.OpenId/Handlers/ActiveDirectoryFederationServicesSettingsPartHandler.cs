using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Handlers {
    [OrchardFeature("Orchard.OpenId.ActiveDirectoryFederationServices")]
    public class ActiveDirectoryFederationServicesSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public ActiveDirectoryFederationServicesSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ActiveDirectoryFederationServicesSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<ActiveDirectoryFederationServicesSettingsPart>("ActiveDirectoryFederationServicesSettings", "Parts.ActiveDirectoryFederationServicesSettings", "OpenId"));
        }
    }
}