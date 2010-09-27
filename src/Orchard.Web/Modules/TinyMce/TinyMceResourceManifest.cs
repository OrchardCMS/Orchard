using Orchard.UI.Resources;

namespace TinyMce {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineScript("TinyMce").SetUrl("tiny_mce.js", "tiny_mce_src.js");
        }
    }
}
