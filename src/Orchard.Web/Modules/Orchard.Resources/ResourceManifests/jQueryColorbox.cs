using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQueryColorbox : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("jQueryColorBox")
                .SetUrl("jQuery.Colorbox/jquery.colorbox.min.js", "jQuery.Colorbox/jquery.colorbox.js")
                .SetVersion("1.6.4")
                .SetDependencies("jQuery");

            manifest.DefineStyle("jQueryColorBox")
                .SetUrl("jQuery.Colorbox/jquery.colorbox.min.css", "jQuery.Colorbox/jquery.colorbox.css")
                .SetVersion("1.6.4");
        }
    }
}
