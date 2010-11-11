using Orchard.Environment.Extensions;
using Orchard.UI.Resources;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("PackagingAdmin").SetUrl("admin.css");
        }
    }
}
