using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.MultiTenancy {
    public class MultiTenancyResourceManifest : ResourceManifest {
        public MultiTenancyResourceManifest() {
            DefineStyle("MultiTenancyAdmin").SetUrl("admin.css");
        }
    }
}
