using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Routable {
    public class RoutableResourceManifest : ResourceManifest {
        public RoutableResourceManifest() {
            DefineScript("Slugify").SetUrl("jquery.slugify.js").SetDependencies("jQuery");
        }
    }
}
