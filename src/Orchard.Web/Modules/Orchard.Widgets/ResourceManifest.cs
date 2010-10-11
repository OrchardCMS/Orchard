using Orchard.UI.Resources;

namespace Orchard.Widgets {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("WidgetsAdmin").SetUrl("admin.css");
        }
    }
}
