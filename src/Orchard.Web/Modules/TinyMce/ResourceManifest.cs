using Orchard.UI.Resources;

namespace TinyMce {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("TinyMce").SetUrl("tiny_mce.js", "tiny_mce_src.js").SetDependencies("jQuery");
            manifest.DefineScript("OrchardTinyMce").SetUrl("orchard-tinymce.js").SetDependencies("TinyMce");
        }
    }
}
