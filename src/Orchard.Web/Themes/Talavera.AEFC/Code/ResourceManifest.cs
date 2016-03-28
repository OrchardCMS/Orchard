using Orchard.UI.Resources;

namespace Talavera.Base {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();            
            manifest.DefineScript("talavera.aefc").SetUrl("talavera.aefc.js").SetDependencies("jQuery","talavera.base");            
        }
    }
}
