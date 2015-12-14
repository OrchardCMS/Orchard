using Orchard.UI.Resources;

namespace Orchard.Modules {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("ModulesAdmin").SetUrl("orchard-modules-admin.css");
        }
    }
}
