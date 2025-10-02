using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQueryUtilities : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("jQueryUtils")
                .SetUrl("jQuery.Utilities/jquery.utils.min.js", "jQuery.Utilities/jquery.utils.js")
                .SetVersion("0.8.5")
                .SetDependencies("jQuery");

            manifest.DefineScript("jQueryPlugin")
                .SetUrl("jQuery.Utilities/jquery.plugin.min.js", "jQuery.Utilities/jquery.plugin.js")
                .SetDependencies("jQuery");

            manifest.DefineScript("jQueryCookie")
                .SetUrl("jQuery.Utilities/jquery.cookie.min.js", "jQuery.Utilities/jquery.cookie.js")
                .SetVersion("1.4.1")
                .SetDependencies("jQuery");
        }
    }
}
