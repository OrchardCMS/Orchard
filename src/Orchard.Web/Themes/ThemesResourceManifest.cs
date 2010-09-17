using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Themes {
    public class ThemesResourceManifest : ResourceManifest {
        public ThemesResourceManifest() {
            DefineStyle("Admin").SetUrl("~/modules/orchard.themes/styles/admin.css");

            DefineScript("ThemeBase").SetUrl("~/modules/orchard.themes/scripts/base.js").SetDependencies("jQuery");
            DefineStyle("Theme").SetUrl("~/modules/orchard.themes/styles/site.css"); // todo: missing
            DefineStyle("ThemeSpecial").SetUrl("~/modules/orchard.themes/styles/special.css");

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

            DefineStyle("SafeMode").SetUrl("~/themes/green/styles/site.css");

            DefineStyle("TheAdmin").SetUrl("~/themes/green/styles/site.css");
            DefineStyle("TheAdmin_IE").SetUrl("~/themes/green/styles/ie.css");
            DefineStyle("TheAdmin_IE6").SetUrl("~/themes/green/styles/ie6.css");
            DefineScript("TheAdmin").SetUrl("~/themes/theadmin/scripts/admin.js").SetDependencies("jQuery");
        }
    }
}
