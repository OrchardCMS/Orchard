using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettingsPart : ContentPart<SiteSettingsPartRecord>, ISite {

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

        public ResourceDebugMode ResourceDebugMode {
            get { return Record.ResourceDebugMode; }
            set { Record.ResourceDebugMode = value; }
        }

        public int PageSize {
            get { return Record.PageSize; }
            set { Record.PageSize = value; }
        }

        [StringLength(255)]
        public string BaseUrl {
            get { return Record.BaseUrl; }
            set { Record.BaseUrl = value; }
        }
    }
}
