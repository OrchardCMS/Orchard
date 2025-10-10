using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;

namespace Orchard.MediaLibrary.WebSearch.Handlers {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchSettingsPartHandler : ContentHandler {
        public BingWebSearchSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<BingWebSearchSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<BingWebSearchSettingsPart>("BingWebSearchSettings", "Parts/WebSearch.BingWebSearchSettings", "media"));
        }
    }
}