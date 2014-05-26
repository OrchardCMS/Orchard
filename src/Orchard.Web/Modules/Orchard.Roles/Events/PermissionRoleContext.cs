using Orchard.Roles.Models;

namespace Orchard.Roles.Events {
    public class PermissionRoleContext : RoleContext {
        public PermissionRecord Permission { get; set; }
    }
}