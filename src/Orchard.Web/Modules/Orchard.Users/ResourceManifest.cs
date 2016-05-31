using Orchard.UI.Resources;

namespace Orchard.Users {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            
            manifest.DefineScript("UsersAdmin").SetUrl("admin-users.min.js", "admin-users.js").SetDependencies("jQuery");
        }
    }
}