using Orchard.Core.Settings.Records;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettings : ContentPart<SiteSettingsRecord>, ISite {
        public static readonly ContentType ContentType = new ContentType{Name="site", DisplayName="Site Settings"};

        public string SiteName {
            get { return Record.SiteName; }
            set { Record.SiteName = value; }
        }
        public string SuperUser { 
            get { return Record.SuperUser; }
            set { Record.SuperUser = value; }
        }
    }
}
