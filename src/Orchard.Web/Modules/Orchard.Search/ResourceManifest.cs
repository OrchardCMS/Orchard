using Orchard.UI.Resources;

namespace Orchard.Search {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("Search").SetUrl("orchard-search-search.css");
        }
    }
}
