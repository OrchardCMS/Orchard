using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests {
    public class jQueryFileUpload : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("jQueryFileUpload")
                .SetUrl("jQuery.FileUpload/jquery.fileupload-full.min.js", "jQuery.FileUpload/jquery.fileupload-full.js")
                .SetVersion("9.11.2")
                .SetDependencies("jQueryUI");
        }
    }
}
