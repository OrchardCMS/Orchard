using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Localization.Resources {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            // todo: move this file
            manifest.DefineStyle("Localization").SetUrl("base.css");
            manifest.DefineStyle("LocalizationAdmin").SetUrl("admin.css");
        }
    }
}
