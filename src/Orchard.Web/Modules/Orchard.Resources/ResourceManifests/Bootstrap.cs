using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class Bootstrap : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineStyle("Bootstrap")
                .SetUrl("Bootstrap/bootstrap.min.css", "Bootstrap/bootstrap.css")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.min.css",
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.css")
                .SetVersion("4.6.2");

            manifest.DefineStyle("Bootstrap.Grid")
                .SetUrl("Bootstrap/bootstrap-grid.min.css", "Bootstrap/bootstrap-grid.css")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap-grid.min.css",
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap-grid.css")
                .SetVersion("4.6.2");

            manifest.DefineStyle("Bootstrap.Reboot")
                .SetUrl("Bootstrap/bootstrap-reboot.min.css", "Bootstrap/bootstrap-reboot.css")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap-reboot.min.css",
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap-reboot.css")
                .SetVersion("4.6.2");

            manifest.DefineScript("Bootstrap")
                .SetUrl("Bootstrap/bootstrap.bundle.min.js", "Bootstrap/bootstrap.bundle.js")
                .SetDependencies("jQuery")
                .SetCdn(
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.bundle.min.js",
                    "//cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.bundle.js")
                .SetVersion("4.6.2");
        }
    }
}
