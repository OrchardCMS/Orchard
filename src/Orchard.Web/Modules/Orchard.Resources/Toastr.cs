using Orchard.UI.Resources;

namespace Orchard.Resources {
    public class Toastr : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Toastr").SetUrl("toastr.min.css", "toastr.css").SetVersion("3.3.5").SetCdn("//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/css/toastr.min.css", "//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/css/toastr.css", true);
            manifest.DefineScript("Toastr").SetUrl("toastr.min.js", "toastr.js").SetVersion("3.3.5").SetDependencies("jQuery").SetCdn("//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/js/toastr.min.js", "//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/js/toastr.js", true);
        }
    }
}
