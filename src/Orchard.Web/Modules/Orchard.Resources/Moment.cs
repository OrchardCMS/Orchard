using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Moment : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Moment").SetUrl("moment.min.js", "moment.js").SetVersion("2.10.6");
        }
    }
}
