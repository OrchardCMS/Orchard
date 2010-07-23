using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.Drivers {
    [UsedImplicitly]
    public class SiteSettingsPartDriver : ContentItemDriver<SiteSettingsPart> {
        protected override ContentType GetContentType() {
            return SiteSettingsPart.ContentType;
        }

        protected override DriverResult Editor(SiteSettingsPart part) {
            return ContentItemTemplate("Items/Settings.Site");
        }
        protected override DriverResult Editor(SiteSettingsPart part, IUpdateModel updater) {
            return ContentItemTemplate("Items/Settings.Site");
        }
    }
}