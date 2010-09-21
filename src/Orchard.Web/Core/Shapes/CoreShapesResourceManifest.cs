using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Shapes {
    public class CoreShapesResourceManifest : ResourceManifest {
        public CoreShapesResourceManifest() {
            DefineScript("ShapesBase").SetUrl("base.js").SetDependencies("jQuery");
            DefineStyle("Shapes").SetUrl("site.css"); // todo: missing
            DefineStyle("ShapesSpecial").SetUrl("special.css");
        }
    }
}
