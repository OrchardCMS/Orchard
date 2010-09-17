using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.UI.Resources;

namespace Orchard.Media {
    public class MediaResourceManifest : ResourceManifest {
        public MediaResourceManifest() {
            DefineStyle("MediaAdmin").SetUrl("admin.css");
        }
    }
}
