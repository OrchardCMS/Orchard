using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Environment.Extensions;

namespace Orchard.MediaLibrary.WebSearch.Models {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Pixabay")]
    public class PixabayWebSearchSettingsPart : ContentPart {
        public string ApiKey {
            get { return this.As<InfosetPart>().Get<PixabayWebSearchSettingsPart>(nameof(ApiKey)); }
            set { this.As<InfosetPart>().Set<PixabayWebSearchSettingsPart>(nameof(ApiKey), value); }
        }
    }
}