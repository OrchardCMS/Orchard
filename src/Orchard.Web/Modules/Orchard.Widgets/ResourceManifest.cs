using Orchard.UI.Resources;

namespace Orchard.Widgets {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("WidgetsAdmin").SetUrl("admin.css");
        }
    }
}
