using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Themes {
    public class ThemesResourceManifest : ResourceManifest {
        public ThemesResourceManifest() {
            DefineStyle("ThemesAdmin").SetUrl("admin.css");
            // todo: used by core\shapes -- move it?
            DefineScript("Switchable").SetUrl("jquery.switchable.js").SetDependencies("jQuery");
            DefineStyle("Switchable").SetUrl("jquery.switchable.css");

            // Resources for the built-in themes (under the orchard.web/themes directory)
            // The manifest would normally go there rather than here, but orchard.web/themes
            // is not registered with AutoFac.
            DefineStyle("Admin").SetUrl("~/modules/orchard.themes/styles/admin.css");

            DefineStyle("Classic").SetUrl("~/themes/classic/styles/site.css");
            DefineStyle("Classic_Blog").SetUrl("~/themes/classic/styles/blog.css");

            DefineStyle("ClassicDark").SetUrl("~/themes/classicdark/styles/site.css");
            DefineStyle("ClassicDark_Blog").SetUrl("~/themes/classicdark/styles/blog.css");

            DefineStyle("Contoso").SetUrl("~/themes/contoso/styles/site.css");
            DefineStyle("Contoso_Search").SetUrl("~/themes/contoso/styles/search.css");

            // todo: include and define the min.js version too
            // todo: move EasySlider to common location
            DefineScript("EasySlider").SetUrl("~/themes/contoso/scripts/easySlider.js").SetDependencies("jQuery");

            DefineStyle("Corporate").SetUrl("~/themes/corporate/styles/site.css");

            DefineStyle("Green").SetUrl("~/themes/green/styles/site.css");
            DefineStyle("Green_Blog").SetUrl("~/themes/green/styles/blog.css");
            DefineStyle("Green_YUI").SetUrl("~/themes/green/styles/yui.css");

            DefineStyle("SafeMode").SetUrl("~/themes/safemode/styles/site.css");

            DefineStyle("TheAdmin").SetUrl("~/themes/theadmin/styles/site.css");
            DefineStyle("TheAdmin_IE").SetUrl("~/themes/theadmin/styles/ie.css");
            DefineStyle("TheAdmin_IE6").SetUrl("~/themes/theadmin/styles/ie6.css");
            DefineScript("TheAdmin").SetUrl("~/themes/theadmin/scripts/admin.js").SetDependencies("jQuery");
        }
    }
}
