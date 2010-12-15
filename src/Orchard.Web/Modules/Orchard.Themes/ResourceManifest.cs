using Orchard.UI.Resources;

namespace Orchard.Themes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("ThemesAdmin").SetUrl("orchard-themes-admin.css");

            // todo: include and define the min.js version too
            // todo: move EasySlider to common location, although it does not appear to be used anywhere right now
            manifest.DefineScript("EasySlider").SetUrl("~/themes/contoso/scripts/easySlider.js").SetDependencies("jQuery");
        }
    }
}
