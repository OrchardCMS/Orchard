using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.Environment.Extensions;
using Orchard.Roles.Events;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.Roles {
    [OrchardFeature("Orchard.AuditTrail.Roles")]
    public class RoleEventHandler : IRoleEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public RoleEventHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
        }

        public void Created(RoleCreatedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Created, context.Role.Name);
        }

        public void Removed(RoleRemovedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Removed, context.Role.Name);
        }

        public void Renamed(RoleRenamedContext context) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", context.Role.Name},
                {"PreviousRoleName", context.PreviousRoleName},
                {"NewRoleName", context.NewRoleName},
            };

            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Renamed, context.Role.Name, properties: null, eventData:eventData);
        }

        public void PermissionAdded(PermissionAddedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.PermissionAdded, context.Role.Name, context.Permission.Name);
        }

        public void PermissionRemoved(PermissionRemovedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.PermissionRemoved, context.Role.Name, context.Permission.Name);
        }

        public void UserAdded(UserAddedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.UserAdded, context.Role.Name, context.User);
        }

        public void UserRemoved(UserRemovedContext context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.UserRemoved, context.Role.Name, context.User);
        }

        private void RecordAuditTrailEvent(string eventName, string roleName) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName}
            };

            RecordAuditTrailEvent(eventName, roleName, properties: null, eventData: eventData);
        }

        private void RecordAuditTrailEvent(string eventName, string roleName, string permissionName) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName},
                {"PermissionName", permissionName}
            };

            RecordAuditTrailEvent(eventName, roleName, properties: null, eventData: eventData);
        }

        private void RecordAuditTrailEvent(string eventName, string roleName, IUser user) {

            var properties = new Dictionary<string, object> {
                {"User", user}
            };

            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName},
                {"UserName", user.UserName}
            };

            RecordAuditTrailEvent(eventName, roleName, properties, eventData);
        }

        private void RecordAuditTrailEvent(string eventName, string roleName, IDictionary<string, object> properties, IDictionary<string, object> eventData) {
            _auditTrailManager.CreateRecord<RoleAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties, eventData, eventFilterKey: "role", eventFilterData: roleName);
        }
    }
}