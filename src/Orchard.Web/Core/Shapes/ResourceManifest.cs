using Orchard.UI.Resources;

namespace Orchard.Core.Shapes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("ShapesBase").SetUrl("base.js").SetDependencies("jQuery");
            manifest.DefineStyle("Shapes").SetUrl("site.css"); // todo: missing
            manifest.DefineStyle("ShapesSpecial").SetUrl("special.css");

            manifest.DefineScript("Html5Shiv").SetUrl("html5.js");

            manifest.DefineScript("Switchable").SetUrl("jquery.switchable.js")
                .SetDependencies("jQuery")
                .SetDependencies("ShapesBase");
            manifest.DefineStyle("Switchable").SetUrl("jquery.switchable.css");
        }
    }
}
