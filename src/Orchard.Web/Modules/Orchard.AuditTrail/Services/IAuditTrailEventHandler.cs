using Orchard.AuditTrail.Services.Models;
using Orchard.Events;

namespace Orchard.AuditTrail.Services {
    public interface IAuditTrailEventHandler : IEventHandler {
        void Create(AuditTrailCreateContext context);
        void Filter(QueryFilterContext context);
        void DisplayFilter(DisplayFilterContext context);
    }
}