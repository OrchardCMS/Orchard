using Orchard.UI.Resources;

namespace Orchard.ContentPicker {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("ContentPicker").SetUrl("ContentPicker.js", "ContentPicker.js").SetDependencies("jQuery");
            manifest.DefineScript("RecentContentTab").SetUrl("RecentContentTab.js", "RecentContentTab.js").SetDependencies("jQuery");
        }
    }
}
