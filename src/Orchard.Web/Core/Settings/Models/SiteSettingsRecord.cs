using Orchard.ContentManagement.Records;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsRecord : ContentPartRecord {
        public virtual string SiteSalt { get; set; }
        public virtual string SiteUrl { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string SuperUser { get; set; }
        public virtual string PageTitleSeparator { get; set; }
        public virtual string HomePage { get; set; }
    }
}