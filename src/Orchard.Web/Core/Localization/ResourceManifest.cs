using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Localization {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("Localization").SetUrl("base.css");
            manifest.DefineStyle("LocalizationAdmin").SetUrl("admin.css");
        }
    }
}
