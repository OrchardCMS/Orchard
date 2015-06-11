using Orchard.UI.Resources;

namespace Orchard.DynamicForms {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("DynamicForms.FormElements").SetUrl("forms-admin.css");
            manifest.DefineScript("DynamicForms.FormElements").SetUrl("Forms.min.js", "Forms.js").SetDependencies("Layouts.LayoutEditor");
        }
    }
}