using Orchard.UI.Resources;

namespace Orchard.Themes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("ThemesAdmin").SetUrl("orchard-themes-admin.css");
        }
    }
}
