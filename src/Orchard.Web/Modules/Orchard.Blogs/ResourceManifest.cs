using Orchard.UI.Resources;

namespace Orchard.Blogs {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("BlogsAdmin").SetUrl("admin.css");
            manifest.DefineStyle("BlogsArchives").SetUrl("archives.css");

            manifest.DefineScript("BlogsArchives").SetUrl("archives.js").SetDependencies("jQuery");
        }
    }
}
