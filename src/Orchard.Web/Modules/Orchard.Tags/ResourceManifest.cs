using Orchard.UI.Resources;

namespace Orchard.Tags {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("TagsAdmin").SetUrl("orchard-tags-admin.css");
        }
    }
}