using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.Role {
    public class RoleEventHandler : IRoleEventHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

        public RoleEventHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
        }

        public void Created(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.RoleCreated, context.Role.Name);
        }

        public void Removed(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.RoleRemoved, context.Role.Name);
        }

        public void Renamed(dynamic context) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", (string)context.Role.Name},
                {"PreviousRoleName", (string)context.PreviousRoleName},
                {"NewRoleName", (string)context.NewRoleName},
            };

            RecordAuditTrail(RoleAuditTrailEventProvider.RoleRenamed, context.Role.Name, properties: null, eventData:eventData);
        }

        public void PermissionAdded(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.PermissionAdded, context.Role.Name, context.Permission.Name);
        }

        public void PermissionRemoved(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.PermissionRemoved, context.Role.Name, context.Permission.Name);
        }

        public void UserAdded(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.UserAdded, context.Role.Name, context.User);
        }

        public void UserRemoved(dynamic context) {
            RecordAuditTrail(RoleAuditTrailEventProvider.UserRemoved, context.Role.Name, context.User);
        }

        private void RecordAuditTrail(string eventName, string roleName) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName}
            };

            RecordAuditTrail(eventName, roleName, properties: null, eventData: eventData);
        }

        private void RecordAuditTrail(string eventName, string roleName, string permissionName) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName},
                {"PermissionName", permissionName}
            };

            RecordAuditTrail(eventName, roleName, properties: null, eventData: eventData);
        }


        private void RecordAuditTrail(string eventName, string roleName, IUser user) {

            var properties = new Dictionary<string, object> {
                {"User", user}
            };

            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName},
                {"UserName", user.UserName}
            };

            RecordAuditTrail(eventName, roleName, properties, eventData);
        }

        private void RecordAuditTrail(string eventName, string roleName, IDictionary<string, object> properties, IDictionary<string, object> eventData) {
            _auditTrailManager.Record<RoleAuditTrailEventProvider>(eventName, _wca.GetContext().CurrentUser, properties, eventData, eventFilterKey: "role", eventFilterData: roleName);
        }
    }
}