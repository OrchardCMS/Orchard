using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Roles {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageRoles = new Permission { Description = "Managing Roles", Name = "ManageRoles" };
        public static readonly Permission AssignRoles = new Permission { Description = "Assign Roles", Name = "AssignRoles", ImpliedBy = new [] { ManageRoles } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageRoles, AssignRoles
            };
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