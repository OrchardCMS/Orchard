using Orchard.Events;

namespace Orchard.Roles.Events {
    public interface IRoleEventHandler : IEventHandler {
        void Created(RoleCreatedContext context);
        void Removed(RoleRemovedContext context);
        void Renamed(RoleRenamedContext context);
        void PermissionAdded(PermissionAddedContext context);
        void PermissionRemoved(PermissionRemovedContext context);
        void UserAdded(UserAddedContext context);
        void UserRemoved(UserRemovedContext context);
    }
}