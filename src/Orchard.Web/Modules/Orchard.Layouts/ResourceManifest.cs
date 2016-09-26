using Orchard.UI.Resources;

namespace Orchard.Layouts {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("Underscore").SetUrl("Lib/underscore.js");
            manifest.DefineScript("Angular").SetUrl("Lib/angular.js").SetDependencies("jQuery");
            manifest.DefineScript("AngularSanitize").SetUrl("Lib/angular-sanitize.js").SetDependencies("Angular");
            manifest.DefineScript("AngularResource").SetUrl("Lib/angular-resource.js").SetDependencies("Angular");
            manifest.DefineScript("Layouts.Models").SetUrl("Models.min.js", "Models.js").SetDependencies("jQuery", "Underscore");

            manifest.DefineScript("Layouts.LayoutEditor").SetUrl("LayoutEditor.min.js", "LayoutEditor.js").SetDependencies("Layouts.Models", "AngularResource", "AngularSanitize", "Angular", "jQuery", "Underscore");
        }
    }
}
