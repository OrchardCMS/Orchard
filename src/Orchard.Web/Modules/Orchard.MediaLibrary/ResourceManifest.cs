using Orchard.UI.Resources;

namespace Orchard.MediaLibrary {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("MediaManagerAdmin").SetUrl("orchard-medialibrary-admin.css").SetDependencies("~/Themes/TheAdmin/Styles/Site.css");
            builder.Add().DefineStyle("FontAwesome").SetUrl("//netdna.bootstrapcdn.com/font-awesome/3.1.1/css/font-awesome.css");
        }
    }
}
