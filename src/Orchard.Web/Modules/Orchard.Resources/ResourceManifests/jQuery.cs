using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQuery : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("jQuery")
                .SetUrl("jQuery/jquery.min.js", "jQuery/jquery.js")
                .SetCdn("//code.jquery.com/jquery-3.7.1.min.js", "//code.jquery.com/jquery-3.7.1.js")
                .SetVersion("3.7.1");
        }
    }
}
