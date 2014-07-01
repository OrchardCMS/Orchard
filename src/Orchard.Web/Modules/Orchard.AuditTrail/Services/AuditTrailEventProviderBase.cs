using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.Services {
    public abstract class AuditTrailEventProviderBase : Component, IAuditTrailEventProvider {
        public abstract void Describe(DescribeContext context);
    }
}