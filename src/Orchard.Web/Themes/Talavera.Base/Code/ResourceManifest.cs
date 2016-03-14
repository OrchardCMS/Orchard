using Orchard.UI.Resources;

namespace Talavera.Base {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap/js/bootstrap.min.js", "bootstrap/js/bootstrap.js").SetVersion("3.3.6").SetDependencies("jQuery");            
            manifest.DefineScript("Custom").SetUrl("custom.js").SetDependencies("jQuery");   
        }
    }
}
