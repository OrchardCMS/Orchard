using Orchard.AuditTrail.Models;
using Orchard.Events;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailEventProvider : IEventHandler {
        void Describe(DescribeContext context);
    }
}