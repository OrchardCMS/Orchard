using Orchard.UI.Resources;

namespace Orchard.DynamicForms {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("DynamicForms.FormElements").SetUrl("DynamicForms-Admin.min.css", "DynamicForms-Admin.css");
            manifest.DefineScript("DynamicForms.FormElements").SetUrl("LayoutEditor.min.js", "LayoutEditor.js").SetDependencies("Layouts.LayoutEditor");
            manifest.DefineScript("DynamicForms.TaxonomyElement").SetUrl("TaxonomyElement.min.js", "TaxonomyElement.js").SetDependencies("jQuery");            
        }
    }
}