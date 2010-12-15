using Orchard.UI.Resources;

namespace Orchard.Core.Routable {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineScript("Slugify").SetUrl("jquery.slugify.js").SetDependencies("jQuery");
        }
    }
}
