using Orchard.UI.Resources;

namespace Themes.Canopy {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Canopy_Bootstrap").SetUrl("bootstrap.min.css", "bootstrap.css");
            manifest.DefineScript("Canopy_Bootstrap").SetUrl("bootstrap.min.js", "bootstrap.js").SetDependencies("jQuery");
        }
    }
}
