using Orchard.ContentManagement.Records;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsPartRecord : ContentPartRecord {
        public virtual string SiteSalt { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string SuperUser { get; set; }
        public virtual string PageTitleSeparator { get; set; }
        public virtual string HomePage { get; set; }
        public virtual string SiteCulture { get; set; }
        public virtual ResourceDebugMode ResourceDebugMode { get; set; }
    }
}