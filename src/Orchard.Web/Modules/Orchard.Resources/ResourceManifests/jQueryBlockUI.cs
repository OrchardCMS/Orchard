using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQueryBlockUI : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("BlockUI")
                .SetUrl("jQuery.BlockUI/jquery.blockui.min.js", "jQuery.BlockUI/jquery.blockui.js")
                .SetVersion("2.70.0")
                .SetDependencies("jQuery");
        }
    }
}
