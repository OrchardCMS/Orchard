using System.Collections.Generic;
using Orchard.AuditTrail.Services;
using Orchard.Environment.Extensions;
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

        public void Created(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Created, context.Role.Name);
        }

        public void Removed(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Removed, context.Role.Name);
        }

        public void Renamed(dynamic context) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", (string)context.Role.Name},
                {"PreviousRoleName", (string)context.PreviousRoleName},
                {"NewRoleName", (string)context.NewRoleName},
            };

            RecordAuditTrailEvent(RoleAuditTrailEventProvider.Renamed, context.Role.Name, properties: null, eventData:eventData);
        }

        public void PermissionAdded(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.PermissionAdded, (string) context.Role.Name, (IUser) context.Permission.Name);
        }

        public void PermissionRemoved(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.PermissionRemoved, (string) context.Role.Name, (IUser) context.Permission.Name);
        }

        public void UserAdded(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.UserAdded, (string) context.Role.Name, (IUser) context.User);
        }

        public void UserRemoved(dynamic context) {
            RecordAuditTrailEvent(RoleAuditTrailEventProvider.UserRemoved, (string) context.Role.Name, (IUser) context.User);
        }

        private void RecordAuditTrailEvent(string eventName, string roleName) {
            var eventData = new Dictionary<string, object> {
                {"RoleName", roleName}
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