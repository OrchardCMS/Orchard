using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ChangeOwner = new Permission { Name = "ChangeOwner", Description = "Change the owner of content items" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ChangeOwner,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ChangeOwner}
                },
            };
        }
    }
}
