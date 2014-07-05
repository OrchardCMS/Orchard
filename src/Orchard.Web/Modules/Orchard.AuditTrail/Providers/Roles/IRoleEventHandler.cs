using Orchard.Events;

namespace Orchard.AuditTrail.Providers.Roles {
    public interface IRoleEventHandler : IEventHandler {
        void Created(dynamic context);
        void Removed(dynamic context);
        void Renamed(dynamic context);
        void PermissionAdded(dynamic context);
        void PermissionRemoved(dynamic context);
        void UserAdded(dynamic context);
        void UserRemoved(dynamic context);
    }
}