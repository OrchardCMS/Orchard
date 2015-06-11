using Orchard.UI.Resources;

namespace Orchard.Comments {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Admin").SetUrl("orchard-comments-admin.css");
        }
    }
}
