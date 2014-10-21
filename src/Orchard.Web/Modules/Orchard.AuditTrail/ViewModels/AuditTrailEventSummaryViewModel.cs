using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailEventSummaryViewModel {
        public AuditTrailEventRecord Record { get; set; }
        public AuditTrailEventDescriptor EventDescriptor { get; set; }
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public dynamic SummaryShape { get; set; }
        public dynamic ActionsShape { get; set; }
    }
}