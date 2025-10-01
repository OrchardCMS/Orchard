using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class AngularJs : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("AngularJs")
                .SetUrl("Angular/angular.min.js", "Angular/angular.js")
                .SetVersion("1.3.3");
            manifest.DefineScript("AngularJs_Sanitize")
                .SetUrl("Angular/angular-sanitize.min.js", "Angular/angular-sanitize.js")
                .SetVersion("1.3.3")
                .SetDependencies("AngularJs");
            manifest.DefineScript("AngularJs_Resource")
                .SetUrl("Angular/angular-resource.min.js", "Angular/angular-resource.js")
                .SetVersion("1.3.3")
                .SetDependencies("AngularJs");
            manifest.DefineScript("AngularJs_Sortable")
                .SetUrl("Angular/angular-sortable.min.js", "Angular/angular-sortable.js")
                .SetDependencies("AngularJs", "jQueryUI_Sortable");

            manifest.DefineScript("AngularJs_Full")
                .SetDependencies("AngularJs", "AngularJs_Sanitize", "AngularJs_Resource", "AngularJs_Sortable");
        }
    }
}
