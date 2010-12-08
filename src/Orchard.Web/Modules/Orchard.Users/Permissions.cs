using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Users {
    [UsedImplicitly]
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageUsers = new Permission { Description = "Manage users", Name = "ManageUsers" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageUsers,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageUsers}
                }
            };
        }

    }
}
