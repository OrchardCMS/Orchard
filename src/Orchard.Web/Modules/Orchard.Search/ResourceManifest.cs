using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Search {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("SearchAdmin").SetUrl("admin.css"); // todo: this does not appear to be used anywhere
            builder.Add().DefineStyle("Search").SetUrl("search.css");
        }
    }
}
