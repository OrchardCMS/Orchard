using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.Controllers {
    public class SiteSettingsDriver : ContentItemDriver<SiteSettings> {
        protected override ContentType GetContentType() {
            return SiteSettings.ContentType;
        }

        protected override DriverResult Editor(SiteSettings part) {
            return ContentItemTemplate("Items/Settings.Site");
        }
        protected override DriverResult Editor(SiteSettings part, IUpdateModel updater) {
            return ContentItemTemplate("Items/Settings.Site");
        }
    }
}
