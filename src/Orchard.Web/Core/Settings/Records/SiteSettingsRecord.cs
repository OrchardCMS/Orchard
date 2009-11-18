using Orchard.Models.Records;

namespace Orchard.Core.Settings.Records {
    public class SiteSettingsRecord : ModelPartRecord {
        public virtual string SiteUrl { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string SuperUser { get; set; }
    }
}