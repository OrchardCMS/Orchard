using Orchard.AuditTrail.Models;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailEventDisplayBuilder : IDependency {
        dynamic BuildDisplay(AuditTrailEventRecord record, string displayType);
        dynamic BuildActions(AuditTrailEventRecord record, string displayType);
    }
}