using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Handlers
{
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectorySettingsPartHandler : ContentHandler
    {
        public Localizer T { get; set; }

        public AzureActiveDirectorySettingsPartHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AzureActiveDirectorySettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<AzureActiveDirectorySettingsPart>("AzureActiveDirectorySettings", "Parts.AzureActiveDirectorySettings", "OpenId"));
        }
    }
}