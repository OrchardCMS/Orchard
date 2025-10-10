using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.MediaLibrary.WebSearch.Models {
    [OrchardFeature("Orchard.MediaLibrary.WebSearch.Google")]
    public class GoogleWebSearchSettingsPart : WebSearchSettingsBase {
        public string SearchEngineId {
            get => this.Retrieve(x => x.SearchEngineId);
            set => this.Store(x => x.SearchEngineId, value);
        }
    }
}