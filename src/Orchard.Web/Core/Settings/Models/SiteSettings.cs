using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettings : ContentPart<SiteSettingsRecord>, ISite {
        public static readonly ContentType ContentType = new ContentType { Name = "site", DisplayName = "Site Settings" };

        public string PageTitleSeparator {
            get { return Record.PageTitleSeparator; }
            set { Record.PageTitleSeparator = value; }
        }
        public string SiteName {
            get { return Record.SiteName; }
            set { Record.SiteName = value; }
        }
        public string SiteSalt {
            get { return Record.SiteSalt; }
        }
        public string SuperUser {
            get { return Record.SuperUser; }
            set { Record.SuperUser = value; }
        }
        public string HomePage {
            get { return Record.HomePage; }
            set { Record.HomePage = value; }
        }
        public string SiteCulture {
            get { return Record.SiteCulture; }
            set { Record.SiteCulture = value; }
        }
    }
}
