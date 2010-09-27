using System;
using Orchard.UI.Resources;

namespace Orchard.Themes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("ThemesAdmin").SetUrl("admin.css");
            // todo: used by core\shapes -- move it?
            manifest.DefineScript("Switchable").SetUrl("jquery.switchable.js").SetDependencies("jQuery");
            manifest.DefineStyle("Switchable").SetUrl("jquery.switchable.css");

            // Resources for the built-in themes (under the orchard.web/themes directory)
            // The manifest would normally go there rather than here, but orchard.web/themes
            // is not registered with AutoFac.
            manifest.DefineStyle("Admin").SetUrl("~/modules/orchard.themes/styles/admin.css");

            manifest.DefineStyle("Classic").SetUrl("~/themes/classic/styles/site.css");
            manifest.DefineStyle("Classic_Blog").SetUrl("~/themes/classic/styles/blog.css");
            
            manifest.DefineStyle("ClassicDark").SetUrl("~/themes/classicdark/styles/site.css");
            manifest.DefineStyle("ClassicDark_Blog").SetUrl("~/themes/classicdark/styles/blog.css");
            
            manifest.DefineStyle("Contoso").SetUrl("~/themes/contoso/styles/site.css");
            manifest.DefineStyle("Contoso_Search").SetUrl("~/themes/contoso/styles/search.css");
            
            // todo: include and define the min.js version too
            // todo: move EasySlider to common location
            manifest.DefineScript("EasySlider").SetUrl("~/themes/contoso/scripts/easySlider.js").SetDependencies("jQuery");

            manifest.DefineStyle("Corporate").SetUrl("~/themes/corporate/styles/site.css");

            manifest.DefineStyle("Green").SetUrl("~/themes/green/styles/site.css");
            manifest.DefineStyle("Green_Blog").SetUrl("~/themes/green/styles/blog.css");
            manifest.DefineStyle("Green_YUI").SetUrl("~/themes/green/styles/yui.css");

            manifest.DefineStyle("SafeMode").SetUrl("~/themes/safemode/styles/site.css");

            manifest.DefineStyle("TheAdmin").SetUrl("~/themes/theadmin/styles/site.css");
            manifest.DefineStyle("TheAdmin_IE").SetUrl("~/themes/theadmin/styles/ie.css");
            manifest.DefineStyle("TheAdmin_IE6").SetUrl("~/themes/theadmin/styles/ie6.css");
            manifest.DefineScript("TheAdmin").SetUrl("~/themes/theadmin/scripts/admin.js").SetDependencies("jQuery");
        }
    }
}
