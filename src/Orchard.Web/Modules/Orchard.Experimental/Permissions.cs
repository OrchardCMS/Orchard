using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Experimental {
    public class Permissions : IPermissionProvider {
        public static readonly Permission DebugShowAllMenuItems = new Permission { Description = "Experimental: Show all menu items", Name = "DebugShowAllMenuItems" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                DebugShowAllMenuItems,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}