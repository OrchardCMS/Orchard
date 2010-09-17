using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Indexing {
    public class IndexingResourceManifest : ResourceManifest {
        public IndexingResourceManifest() {
            DefineStyle("IndexingAdmin").SetUrl("admin.css"); // todo: this does not exist
        }
    }
}
