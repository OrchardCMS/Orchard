using Orchard.UI.Resources;

namespace Talavera.Base {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery-ui").SetUrl("jquery-ui-1.11.4.js").SetDependencies("jQuery");
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap/js/bootstrap.min.js", "bootstrap/js/bootstrap.js").SetVersion("3.3.6").SetDependencies("jQuery");            
            manifest.DefineScript("talavera.base").SetUrl("talavera.base.js").SetDependencies("jQuery");
            manifest.DefineScript("prefixfree").SetUrl("prefixfree.min.js");
        }
    }
}
