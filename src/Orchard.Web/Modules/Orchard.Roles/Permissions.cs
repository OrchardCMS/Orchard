using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Extensions.Models;
using Orchard.Roles.Constants;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    public class Permissions : IPermissionProvider {
        private readonly IRepository<RoleRecord> _roleRepository;

        public static readonly Permission ManageRoles = new Permission { Description = "Managing Roles", Name = "ManageRoles" };
        public static readonly Permission AssignRoles = new Permission { Description = "Assign Roles", Name = "AssignRoles", ImpliedBy = new[] { ManageRoles } };

        public virtual Feature Feature { get; set; }

        private static readonly Permission AssignRoleTemplate =
            new Permission {
                Description = "Assign Role - {0}",
                Name = "AssignRole_{0}",
                ImpliedBy = new[] { ManageRoles, AssignRoles }
            };

        public Permissions(
            // A dependency on IRoleService to get the list of roles would lead to a
            // circular dependency, because that service has methods to handle the
            // permissions for each specific role.
            IRepository<RoleRecord> roleRepository) {

            _roleRepository = roleRepository;
        }

        public static Permission CreatePermissionForAssignRole(string roleName) {
            return new Permission {
                Description = string.Format(AssignRoleTemplate.Description, roleName),
                Name = string.Format(AssignRoleTemplate.Name, roleName),
                ImpliedBy = AssignRoleTemplate.ImpliedBy
            };
        }

        private IEnumerable<Permission> GetAssignRolePermissions() {
            var allRoleNames = _roleRepository.Table
                .Select(r => r.Name)
                .ToList()
                // Never have to assign Anonymous or Authenticated roles
                .Except(SystemRoles.GetSystemRoles());
            foreach (var roleName in allRoleNames) {
                yield return CreatePermissionForAssignRole(roleName);
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            yield return ManageRoles;
            yield return AssignRoles;
            foreach (var permission in GetAssignRolePermissions()) {
                yield return permission;
            }
            yield break;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageRoles, AssignRoles}
                },
            };
        }

    }
}