using Orchard.UI.Resources;

namespace Orchard.ContentTypes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("ContentTypesAdmin").SetUrl("orchard-contenttypes-admin.css");
        }
    }
}
