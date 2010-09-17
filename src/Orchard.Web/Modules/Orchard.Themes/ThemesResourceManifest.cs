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
        }
    }
}
