using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailCategoryDescriptor {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public IEnumerable<AuditTrailEventDescriptor> Events { get; set; }
    }
}