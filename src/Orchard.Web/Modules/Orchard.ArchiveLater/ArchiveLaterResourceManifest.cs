using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.ArchiveLater {
    public class ArchiveLaterResourceManifest : ResourceManifest {
        public ArchiveLaterResourceManifest() {
            DefineStyle("ArchiveLater_DatePicker").SetUrl("datetime.css");
        }
    }
}
