using Orchard.AuditTrail.Models;
using Orchard.Events;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailEventHandler : IEventHandler {
        void Create(AuditTrailCreateContext context);
    }
}