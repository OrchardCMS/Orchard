using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Bootstrap : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Bootstrap").SetUrl("bootstrap.min.css", "bootstrap.css").SetVersion("4.3.1").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/4.3.1/css/bootstrap.min.css", "//ajax.aspnetcdn.com/ajax/bootstrap/4.3.1/css/bootstrap.css");
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap.min.js", "bootstrap.js").SetVersion("4.3.1").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/4.3.1/bootstrap.bundle.min.js", "//ajax.aspnetcdn.com/ajax/bootstrap/4.3.1/bootstrap.bundle.js");
        }
    }
}
