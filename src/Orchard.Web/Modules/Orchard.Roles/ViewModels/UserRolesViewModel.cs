using System.Collections.Generic;
using Orchard.Roles.Models;
using Orchard.Security;

namespace Orchard.Roles.ViewModels {
    public class UserRolesViewModel {
        public UserRolesViewModel() {
            Roles = new List<UserRoleEntry>();
        }

        public IUser User { get; set; }
        public IUserRoles UserRoles { get; set; }
        public IList<UserRoleEntry> Roles { get; set; }
    }

    public class UserRoleEntry {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public bool Granted { get; set; }
    }
}
