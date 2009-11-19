using Orchard.Core.Settings.Records;
using Orchard.Models;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettings : ContentPartForRecord<SiteSettingsRecord>, ISite {
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
