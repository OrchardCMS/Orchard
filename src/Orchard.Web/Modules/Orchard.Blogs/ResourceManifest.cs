using Orchard.UI.Resources;

namespace Orchard.Blogs {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("BlogsAdmin").SetUrl("orchard-blogs-admin.css");
            manifest.DefineStyle("BlogsArchives").SetUrl("orchard-blogs-archives.css");

            manifest.DefineScript("BlogsArchives").SetUrl("orchard-blogs-archives.js").SetDependencies("jQuery");
        }
    }
}
