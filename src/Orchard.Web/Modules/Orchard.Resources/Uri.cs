using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Uri : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Uri").SetUrl("uri.min.js", "uri.js").SetVersion("1.16.1");
        }
    }
}
