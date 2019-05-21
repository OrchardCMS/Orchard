using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Environment.Extensions;

namespace Orchard.MediaLibrary.WebSearch.Models {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Bing")]
    public class BingWebSearchSettingsPart : ContentPart {
        public string ApiKey {
            get { return this.As<InfosetPart>().Get<BingWebSearchSettingsPart>(nameof(ApiKey)); }
            set { this.As<InfosetPart>().Set<BingWebSearchSettingsPart>(nameof(ApiKey), value); }
        }
    }
}