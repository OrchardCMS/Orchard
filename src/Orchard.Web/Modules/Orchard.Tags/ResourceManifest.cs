using Orchard.UI.Resources;

namespace Orchard.Tags {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("TagsAutocomplete").SetUrl("orchard-tags-autocomplete.js").SetDependencies("jQueryUI");

            manifest.DefineStyle("TagsAdmin").SetUrl("orchard-tags-admin.css");
        }
    }
}