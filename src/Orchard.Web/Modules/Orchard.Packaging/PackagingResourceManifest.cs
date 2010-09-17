using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Packaging {
    public class PackagingResourceManifest : ResourceManifest {
        public PackagingResourceManifest() {
            DefineStyle("PackagingAdmin").SetUrl("admin.css");
        }
    }
}
