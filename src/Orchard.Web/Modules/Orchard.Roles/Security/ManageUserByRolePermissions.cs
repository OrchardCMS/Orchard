using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Extensions.Models;
using Orchard.Roles.Constants;
using Orchard.Roles.Models;
using Orchard.Security.Permissions;
using UserPermissions = Orchard.Users.Permissions;

namespace Orchard.Roles.Security {
    public class ManageUserByRolePermissions : IPermissionProvider {
        private readonly IRepository<RoleRecord> _roleRepository;

        public virtual Feature Feature { get; set; }

        private static readonly Permission ManageUsersInRoleTemplate =
            new Permission {
                Description = "Manage Users in Role - {0}",
                Name = "ManageUsersInRole_{0}",
                ImpliedBy = new[] { UserPermissions.ManageUsers }
            };

        public ManageUserByRolePermissions(
            // A dependency on IRoleService to get the list of roles would lead to a
            // circular dependency, because that service has methods to handle the
            // permissions for each specific role.
            IRepository<RoleRecord> roleRepository) {

            _roleRepository = roleRepository;
        }

        public static Permission CreatePermissionForManageUsersInRole(string roleName) {
            return new Permission {
                Description = string.Format(ManageUsersInRoleTemplate.Description, roleName),
                Name = string.Format(ManageUsersInRoleTemplate.Name, roleName),
                ImpliedBy = ManageUsersInRoleTemplate.ImpliedBy
            };
        }


        private IEnumerable<Permission> GetManageUsersInRolePermissions() {
            var allRoleNames = _roleRepository.Table
                .Select(r => r.Name)
                .ToList()
                // Never have to manage explicitly Anonymous or Authenticated roles
                .Except(SystemRoles.GetSystemRoles());
            foreach (var roleName in allRoleNames) {
                yield return CreatePermissionForManageUsersInRole(roleName);
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            foreach (var permission in GetManageUsersInRolePermissions()) {
                yield return permission;
            }
            yield break;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}