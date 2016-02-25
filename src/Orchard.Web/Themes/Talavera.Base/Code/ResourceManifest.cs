using Orchard.UI.Resources;

namespace Talavera.Base {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("Bootstrap").SetUrl("bootstrap/js/bootstrap.min.js", "bootstrap/js/bootstrap.js").SetVersion("3.3.6").SetDependencies("jQuery");
            manifest.DefineScript("HoverDropdown").SetUrl("hover-dropdown.js").SetDependencies("Bootstrap");
            manifest.DefineScript("Custom").SetUrl("custom.js").SetDependencies("jQuery");

            //manifest.DefineScript("Stapel-Modernizr").SetUrl("stapel/modernizr.custom.63321.js");
            //manifest.DefineScript("Stapel").SetUrl("stapel/jquery.stapel.js").SetDependencies("jQuery", "Stapel-Modernizr");
            //manifest.DefineScript("prettyPhoto").SetUrl("prettyPhoto/jquery.prettyPhoto.js").SetDependencies("jQuery");
            //manifest.DefineStyle("Stapel").SetUrl("stapel/stapel.css");
            //manifest.DefineStyle("prettyPhoto").SetUrl("prettyPhoto/prettyPhoto.css");        
        }
    }
}
