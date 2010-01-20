using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Security.Permissions;

namespace Orchard.Users {
    [UsedImplicitly]
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageUsers = new Permission { Description = "Manage users", Name = "ManageUsers" };
        public static readonly Permission AddUsers = new Permission { Description = "Add users", Name = "AddUsers" };

        public string PackageName {
            get {
                return "Users";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageUsers,
                AddUsers,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}
