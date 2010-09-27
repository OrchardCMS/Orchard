using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Shapes {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("ShapesBase").SetUrl("base.js").SetDependencies("jQuery");
            manifest.DefineStyle("Shapes").SetUrl("site.css"); // todo: missing
            manifest.DefineStyle("ShapesSpecial").SetUrl("special.css");
        }
    }
}
