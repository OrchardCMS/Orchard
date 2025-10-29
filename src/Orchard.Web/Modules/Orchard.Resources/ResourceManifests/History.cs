using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class History : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("History").SetUrl("History/history.min.js", "History/history.js");
        }
    }
}
