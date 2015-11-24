using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class FontAwesome : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("FontAwesome").SetUrl("font-awesome.min.css", "font-awesome.css").SetVersion("4.4.0").SetCdn("//maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css", "//maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.css", true);
        }
    }
}