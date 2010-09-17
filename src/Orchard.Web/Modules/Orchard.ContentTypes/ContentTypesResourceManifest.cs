using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.ContentTypes {
    public class ContentTypesResourceManifest : ResourceManifest {
        public ContentTypesResourceManifest() {
            DefineStyle("ContentTypesAdmin").SetUrl("admin.css");
        }
    }
}
