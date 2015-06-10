using Orchard.UI.Resources;

namespace Orchard.DynamicForms {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("DynamicForms.FormElements").SetUrl("forms-admin.min.css", "forms-admin.css");
            manifest.DefineScript("DynamicForms.FormElements").SetUrl("LayoutEditor.min.js", "LayoutEditor.js").SetDependencies("Layouts.LayoutEditor");
        }
    }
}