using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.WebSearch.Models;

namespace Orchard.MediaLibrary.WebSearch.Handlers {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Pixabay")]
    public class PixabayWebSearchSettingsPartHandler : ContentHandler {
        public PixabayWebSearchSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<PixabayWebSearchSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<PixabayWebSearchSettingsPart>("PixabayWebSearchSettings", "Parts/WebSearch.PixabayWebSearchSettings", "media"));
        }
    }
}