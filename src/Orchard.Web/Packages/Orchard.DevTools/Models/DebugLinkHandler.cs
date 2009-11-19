using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.DevTools.Models {

    public class ShowDebugLink {
        public ContentItem ContentItem { get; set; }
    }

    public class DebugLinkHandler : ContentHandler {
        protected override void GetDisplays(GetDisplaysContext context) {
            context.Displays.Add(new ModelTemplate { Model = new ShowDebugLink { ContentItem = context.ContentItem } });
        }
        protected override void GetEditors(GetEditorsContext context) {
            context.Editors.Add(new ModelTemplate { Model = new ShowDebugLink { ContentItem = context.ContentItem } });
        }
    }
}
