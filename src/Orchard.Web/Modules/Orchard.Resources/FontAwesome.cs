using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Orchard.Resources {
    [OrchardFeature("Orchard.Resources.FontAwesome")]
    public class FontAwesome : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("FontAwesome").SetUrl("font-awesome.min.css", "font-awesome.css").SetVersion("4.3.0");
        }
    }
}
