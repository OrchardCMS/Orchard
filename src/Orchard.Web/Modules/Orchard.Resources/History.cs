using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Orchard.Resources {
    [OrchardFeature("Orchard.Resources.History")]
    public class History : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("History").SetUrl("history.min.js", "history.js");
        }
    }
}
