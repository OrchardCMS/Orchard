using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.Roles {
    [OrchardFeature("Orchard.AuditTrail.Roles")]
    public class RoleAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string Renamed = "Renamed";
        public const string PermissionAdded = "PermissionAdded";
        public const string PermissionRemoved = "PermissionRemoved";
        public const string UserAdded = "UserAdded";
        public const string UserRemoved = "UserRemoved";

        public override void Describe(DescribeContext context) {
            context.For("Role", T("Roles"))
                .Event(this, Created, T("Created"), T("A role was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("A role was removed."), enableByDefault: true)
                .Event(this, Renamed, T("Renamed"), T("A role was renamed."), enableByDefault: true)
                .Event(this, PermissionAdded, T("Permission added"), T("A permission was added to a role."), enableByDefault: true)
                .Event(this, PermissionRemoved, T("Permission removed"), T("A permission was removed from a role."), enableByDefault: true)
                .Event(this, UserAdded, T("User added"), T("A user was added to a role."), enableByDefault: true)
                .Event(this, UserRemoved, T("User removed"), T("A user was removed from a role."), enableByDefault: true);
        }
    }
}