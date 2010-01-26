using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    [UsedImplicitly]
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageRoles = new Permission { Description = "Create and manage roles", Name = "ManageRoles" };
        public static readonly Permission ApplyRoles = new Permission { Description = "Assign users to roles", Name = "AssignUsersToRoles", ImpliedBy = new[] { ManageRoles } };

        public string PackageName {
            get {
                return "Roles";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageRoles,
                ApplyRoles,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrators",
                    Permissions = new[] {ManageRoles, ApplyRoles}
                }
            };
        }
    }
}
