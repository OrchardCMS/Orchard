using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    [UsedImplicitly]
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageRoles = new Permission { Description = "Create and manage roles", Name = "ManageRoles" };
        public static readonly Permission AssignUsersToRoles = new Permission { Description = "Assign users to roles", Name = "AssignUsersToRoles" };

        public string PackageName {
            get {
                return "Roles";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageRoles,
                AssignUsersToRoles,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }

}
