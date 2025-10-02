using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class Knockout : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Knockout")
                .SetUrl("Knockout/knockout.min.js", "Knockout/knockout.js")
                // TODO: Set the CDN URL as soon as its available on the AJAXCDN site for this version.
                // 3.5.1 is not available, only 3.5.0.
                .SetVersion("3.5.1");
        }
    }
}
