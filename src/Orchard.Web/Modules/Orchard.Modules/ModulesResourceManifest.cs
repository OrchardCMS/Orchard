using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Modules {
    public class ModulesResourceManifest : ResourceManifest {
        public ModulesResourceManifest() {
            DefineStyle("ModulesAdmin").SetUrl("admin.css");
        }
    }
}
