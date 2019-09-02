using System;
using Orchard.Core.Settings.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Settings.Handlers {
    public class SiteSettingsPartHandler : ContentHandler {
        public SiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<SiteSettingsPart>("Site"));

            OnInitializing<SiteSettingsPart>(InitializeSiteSettings);
        }

        private static void InitializeSiteSettings(InitializingContentContext initializingContentContext, SiteSettingsPart siteSettingsPart) {
            siteSettingsPart.SiteSalt = Guid.NewGuid().ToString("N");
            siteSettingsPart.SiteName = "My Orchard Project Application";
            siteSettingsPart.PageTitleSeparator = " - ";
            siteSettingsPart.SiteTimeZone = TimeZoneInfo.Local.Id;
            siteSettingsPart.UseFileHash = true;
        }
    }
}