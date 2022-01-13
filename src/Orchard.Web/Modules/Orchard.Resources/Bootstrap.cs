using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Bootstrap : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Bootstrap").SetUrl("bootstrap.min.css", "bootstrap.css").SetVersion("4.6.0").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/4.6.0/css/bootstrap.min.css", "//ajax.aspnetcdn.com/ajax/bootstrap/4.6.0/css/bootstrap.css");
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap.bundle.min.js", "bootstrap.bundle.js").SetVersion("4.6.0").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/4.6.0/bootstrap.bundle.min.js", "//ajax.aspnetcdn.com/ajax/bootstrap/4.6.0/bootstrap.bundle.js");
        }
    }
}
