using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Localization.Resources {
    public class LocalizationResourceManifest : ResourceManifest {
        public LocalizationResourceManifest() {
            // todo: move this file
            DefineStyle("Localization").SetUrl("base.css");
            DefineStyle("LocalizationAdmin").SetUrl("admin.css");
        }
    }
}
