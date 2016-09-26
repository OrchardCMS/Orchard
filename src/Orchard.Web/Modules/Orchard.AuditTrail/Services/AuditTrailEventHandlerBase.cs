using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailEventHandlerBase : Component, IAuditTrailEventHandler {
        public virtual void Create(AuditTrailCreateContext context) {}
        public virtual void Filter(QueryFilterContext context) {}
        public virtual void DisplayFilter(DisplayFilterContext context) { }
    }
}