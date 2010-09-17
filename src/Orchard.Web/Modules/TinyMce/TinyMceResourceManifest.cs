using Orchard.UI.Resources;

namespace TinyMce {
    public class TinyMceResourceManifest : ResourceManifest {
        public TinyMceResourceManifest() {
            DefineScript("TinyMce").SetUrl("tiny_mce.js", "tiny_mce_src.js");
        }
    }
}
