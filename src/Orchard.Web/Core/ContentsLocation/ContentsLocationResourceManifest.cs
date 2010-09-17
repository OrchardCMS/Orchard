using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.ContentsLocation {
    public class ContentsLocationResourceManifest : ResourceManifest {
        public ContentsLocationResourceManifest() {
            DefineStyle("ContentsLocationAdmin").SetUrl("admin.css");
        }
    }
}
