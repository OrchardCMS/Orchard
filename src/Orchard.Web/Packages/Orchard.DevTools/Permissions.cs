using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security.Permissions;

namespace Orchard.DevTools {
    public class Permissions : IPermissionProvider {
        public static readonly Permission DebugShowAllMenuItems = new Permission { Description = "DevTools: Show all menu items", Name = "DebugShowAllMenuItems" };

        public string PackageName {
            get {
                return "DevTools";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                DebugShowAllMenuItems,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}
