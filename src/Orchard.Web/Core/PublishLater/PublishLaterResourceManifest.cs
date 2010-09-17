using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.PublishLater {
    public class PublishLaterResourceManifest : ResourceManifest {
        public PublishLaterResourceManifest() {
            DefineStyle("PublishLater_DatePicker").SetUrl("datetime.css");
        }
    }
}
