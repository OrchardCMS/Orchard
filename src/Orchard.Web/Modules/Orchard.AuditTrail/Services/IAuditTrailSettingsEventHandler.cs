using Orchard.AuditTrail.Services.Models;
using Orchard.Events;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailSettingsEventHandler : IEventHandler {
        void Updated(AuditTrailSettingsContext context);
    }
}