using Orchard.OpenId.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Handlers {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectorySettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public AzureActiveDirectorySettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AzureActiveDirectorySettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<AzureActiveDirectorySettingsPart>("AzureActiveDirectorySettings", "Parts.AzureActiveDirectorySettings", "OpenId"));
        }
    }
}