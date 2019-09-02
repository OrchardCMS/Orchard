using Orchard.UI.Resources;

namespace Orchard.MultiTenancy {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("MultiTenancyAdmin").SetUrl("orchard-multitenancy-admin.css");
        }
    }
}
