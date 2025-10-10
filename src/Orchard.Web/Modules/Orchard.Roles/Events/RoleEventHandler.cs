using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Workflows.Services;

namespace Orchard.Roles.Events {
    [OrchardFeature("Orchard.Roles.Workflows")]
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
                    { "Action", "Created" } });
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
                    { "Action", "Removed" } });
        }

        public void Renamed(RoleRenamedContext context) {
            _workflowManager.TriggerEvent("OnRoleEvent",
               null,
               () => new Dictionary<string, object> {
                    { "PreviousName", context.PreviousRoleName },
                    { "NewName", context.NewRoleName },
                    { "Action", "Renamed" } });
        }

        public void UserAdded(UserAddedContext context) {
            // Content of workflow event is the user
            var content = context.User.ContentItem;
            _workflowManager.TriggerEvent("OnRoleEvent",
                content,
                () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "User", context.User },
                    { "Action", "UserAdded" } });
        }

        public void UserRemoved(UserRemovedContext context) {
            // Content of workflow event is the user
            var content = context.User.ContentItem;
            _workflowManager.TriggerEvent("OnRoleEvent",
               content,
               () => new Dictionary<string, object> {
                    { "Role", context.Role },
                    { "User", context.User },
                    { "Action", "UserRemoved" } });
        }
    }
}