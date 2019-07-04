using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;

namespace Orchard.MediaLibrary.WebSearch.Handlers {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Google")]
    public class GoogleWebSearchSettingsPartHandler : ContentHandler {
        public GoogleWebSearchSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<GoogleWebSearchSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<GoogleWebSearchSettingsPart>("GoogleWebSearchSettings", "Parts/WebSearch.GoogleWebSearchSettings", "media"));
        }
    }
}