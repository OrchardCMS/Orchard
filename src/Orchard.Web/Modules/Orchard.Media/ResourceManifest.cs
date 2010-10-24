using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.UI.Resources;

namespace Orchard.Media {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("MediaAdmin").SetUrl("admin.css");
        }
    }
}
