using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Search {
    public class SearchResourceManifest : ResourceManifest {
        public SearchResourceManifest() {
            DefineStyle("SearchAdmin").SetUrl("admin.css"); // todo: this does not appear to be used anywhere
            DefineStyle("Search").SetUrl("search.css");
        }
    }
}
