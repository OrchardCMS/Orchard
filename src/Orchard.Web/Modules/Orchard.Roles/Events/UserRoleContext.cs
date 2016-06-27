using Orchard.Security;

namespace Orchard.Roles.Events {
    public class UserRoleContext : RoleContext {
        public IUser User { get; set; }
    }
}