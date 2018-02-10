using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class BlockUI : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("BlockUI").SetUrl("jquery.blockui.min.js", "jquery.blockui.js").SetVersion("2.70.0").SetDependencies("jQuery");
        }
    }
}
