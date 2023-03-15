using System.Collections.Generic;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Events {
    public class RoleEventHandler : IRoleEventHandler {
        private readonly IWorkflowManager _workflowManager;

        public RoleEventHandler(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
        }

        public void Created(RoleCreatedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "Action", "RoleCreated" } });
        }

        public void PermissionAdded(PermissionAddedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "Permission", context.Permission },
                    { "Action", "PermissionAdded" } });
        }

        public void PermissionRemoved(PermissionRemovedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "Permission", context.Permission },
                    { "Action", "PermissionRemoved" } });
        }

        public void Removed(RoleRemovedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "Action", "RoleRemoved" } });
        }

        public void Renamed(RoleRenamedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "PreviousName", context.PreviousRoleName },
                    { "NewName", context.NewRoleName },
                    { "Action", "RoleRenamed" } });
        }

        public void UserAdded(UserAddedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
                null,
                () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "User", context.User },
                    { "Action", "UserAdded" } });
        }

        public void UserRemoved(UserRemovedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "User", context.User },
                    { "Action", "UserRemoved" } });
        }
    }
}