using Orchard.UI.Resources;

namespace Orchard.ContentPicker {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("ContentPicker").SetUrl("ContentPicker.js", "ContentPicker.js").SetDependencies("jQuery");
            manifest.DefineScript("SelectableContentTab").SetUrl("SelectableContentTab.js", "SelectableContentTab.js").SetDependencies("jQuery");
        }
    }
}
