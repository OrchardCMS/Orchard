using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class Underscore : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Underscore").SetUrl("underscore.min.js", "underscore.js").SetVersion("1.7.0");
        }
    }
}
