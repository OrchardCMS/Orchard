using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Models {
    public class SiteSettings2PartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string BaseUrl { get; set; }
    }
}