using Orchard.UI.Resources;

namespace Orchard.MediaLibrary {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("MediaManagerAdmin").SetUrl("orchard-medialibrary-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");
        }
    }
}