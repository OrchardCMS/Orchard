using Orchard.UI.Resources;

namespace Orchard.ContentTypes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("ContentTypesAdmin").SetUrl("orchard-contenttypes-admin.min.css", "orchard-contenttypes-admin.css");

            manifest.DefineScript("ContentTypesAdmin").SetUrl("admin-contenttypes.min.js", "admin-contenttypes.js").SetDependencies("jQuery");
        }
    }
}