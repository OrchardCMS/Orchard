using Orchard.ContentManagement;

namespace Orchard.MediaLibrary.WebSearch.Models {
    public abstract class WebSearchSettingsBase : ContentPart, IWebSearchSettings {
        public string ApiKey {
            get => this.Retrieve(x => x.ApiKey);
            set => this.Store(x => x.ApiKey, value);
        }
    }
}