using Orchard.AuditTrail.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailDetailsViewModel {
        public AuditTrailEventRecord Record { get; set; }
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public dynamic DetailsShape { get; set; }
    }
}