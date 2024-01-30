using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Bootstrap : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Bootstrap")
                .SetUrl("bootstrap.min.css", "bootstrap.css")
                .SetCdn("//cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css", "//cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.css")
                .SetVersion("4.6.1");
            manifest.DefineScript("Bootstrap").SetUrl("bootstrap.bundle.min.js", "bootstrap.bundle.js")
                .SetDependencies("jQuery")
                .SetCdn("//cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.min.js", "//cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.bundle.js")
                .SetVersion("4.6.1");
        }
    }
}
