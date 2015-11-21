using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Orchard.Resources {
    [OrchardFeature("Orchard.Resources.Moment")]
    public class Moment : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Moment").SetUrl("moment.min.js", "moment.js").SetVersion("2.10.6");
        }
    }
}
