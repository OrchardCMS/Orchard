using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.OpenId {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageOpenId = new Permission { Description = "Manage OpenId settings", Name = "ManageOpenId" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageOpenId,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageOpenId}
                },
            };
        }

    }
}