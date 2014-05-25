using Orchard.Localization;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailEventDescriptor {
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public string Event { get; set; }
        public LocalizedString Name { get; set; }
        public LocalizedString Description { get; set; }
        public bool IsEnabledByDefault { get; set; }
    }
}