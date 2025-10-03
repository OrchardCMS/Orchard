using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class Uri : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Uri").SetUrl("Uri/uri.min.js", "Uri/uri.js").SetVersion("1.16.1");
        }
    }
}
