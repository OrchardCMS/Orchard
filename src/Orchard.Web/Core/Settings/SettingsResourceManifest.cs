using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.Settings {
    public class SettingsResourceManifest : ResourceManifest {
        public SettingsResourceManifest() {
            DefineStyle("SettingsAdmin").SetUrl("admin.css");
        }
    }
}
