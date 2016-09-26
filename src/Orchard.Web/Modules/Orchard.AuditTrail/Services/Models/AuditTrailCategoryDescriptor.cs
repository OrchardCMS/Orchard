using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.Localization;

namespace Orchard.AuditTrail.Services.Models {
    public class AuditTrailCategoryDescriptor {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public IEnumerable<AuditTrailEventDescriptor> Events { get; set; }
    }
}