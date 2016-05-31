using Orchard.UI.Resources;

namespace Orchard.Roles {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            
            manifest.DefineScript("RolesAdmin").SetUrl("admin-roles.min.js", "admin-roles.js").SetDependencies("jQuery");
        }
    }
}