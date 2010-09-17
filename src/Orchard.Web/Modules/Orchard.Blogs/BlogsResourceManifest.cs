using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Blogs {
    public class BlogsResourceManifest : ResourceManifest {
        public BlogsResourceManifest() {
            DefineStyle("BlogsAdmin").SetUrl("admin.css");
            DefineStyle("BlogsArchives").SetUrl("archives.css");

            DefineScript("BlogsArchives").SetUrl("archives.js").SetDependencies("jQuery");
        }
    }
}
