using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Handlers {
    [OrchardFeature("Orchard.OpenId.Facebook")]
    public class FacebookSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public FacebookSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<FacebookSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<FacebookSettingsPart>("FacebookSettings", "Parts.FacebookSettings", "OpenId"));
        }
    }
}