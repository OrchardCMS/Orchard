using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Bootstrap : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Bootstrap").SetUrl("bootstrap.min.css", "bootstrap.css").SetVersion("3.3.5").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/3.3.5/css/bootstrap.min.css", "//ajax.aspnetcdn.com/ajax/bootstrap/3.3.5/css/bootstrap.css", true);
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap.min.js", "bootstrap.js").SetVersion("3.3.5").SetDependencies("jQuery").SetCdn("//ajax.aspnetcdn.com/ajax/bootstrap/3.3.5/bootstrap.min.js", "//ajax.aspnetcdn.com/ajax/bootstrap/3.3.5/bootstrap.js", true);
        }
    }
}
