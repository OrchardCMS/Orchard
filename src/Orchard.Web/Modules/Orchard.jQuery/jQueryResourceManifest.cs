namespace Orchard.UI.Resources {
    public class jQueryResourceManifest : ResourceManifest {
        public jQueryResourceManifest() {
            DefineScript("jQuery").SetUrl("jquery-1.4.2.min.js", "jquery-1.4.2.js").SetVersion("1.4.2");
        }
    }
}
