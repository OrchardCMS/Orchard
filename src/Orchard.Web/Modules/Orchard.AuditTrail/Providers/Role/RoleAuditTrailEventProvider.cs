using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;

namespace Orchard.AuditTrail.Providers.Role {
    public class RoleAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string RoleCreated = "RoleCreated";
        public const string RoleRemoved = "RoleRemoved";
        public const string RoleRenamed = "RoleRenamed";
        public const string PermissionAdded = "PermissionAdded";
        public const string PermissionRemoved = "PermissionRemoved";
        public const string UserAdded = "UserAdded";
        public const string UserRemoved = "UserRemoved";

        public override void Describe(DescribeContext context) {
            context.For("Role", T("Role"))
                .Event(this, RoleCreated, T("Role created"), T("A role was created."), enableByDefault: true)
                .Event(this, RoleRemoved, T("Role removed"), T("A role was removed."), enableByDefault: true)
                .Event(this, RoleRenamed, T("Role renamed"), T("A role was renamed."), enableByDefault: true)
                .Event(this, PermissionAdded, T("Permission added"), T("Permission was added to a role."), enableByDefault: true)
                .Event(this, PermissionRemoved, T("Permission removed"), T("Permission was removed from a role."), enableByDefault: true)
                .Event(this, UserAdded, T("User added"), T("A user was added to a role."), enableByDefault: true)
                .Event(this, UserRemoved, T("User removed"), T("A user was removed from a role."), enableByDefault: true);
        }
    }
}