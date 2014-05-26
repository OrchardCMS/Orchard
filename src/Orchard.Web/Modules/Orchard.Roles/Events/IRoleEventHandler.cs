using Orchard.Events;

namespace Orchard.Roles.Events {
    public interface IRoleEventHandler : IEventHandler {
        void Created(RoleCreatedContext context);
        void Removed(RoleRemovedContext context);
        void Renamed(RoleRenamedContext context);
        void PermissionsChanged(PermissionAddedContext context);
        void UserAdded(UserAddedContext context);
        void UserRemoved(UserRemovedContext context);
    }
}