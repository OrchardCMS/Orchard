using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Core.Themes {
    public class Permissions : IPermissionProvider {
        public static readonly Permission InstallUninstallTheme = new Permission { Description = "Installing or Uninstalling Themes", Name = "InstallUninstallTheme" };
        public static readonly Permission SetSiteTheme = new Permission { Description = "Setting the Current Theme", Name = "SetSiteTheme" };

        public string PackageName {
            get {
                return "Themes";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                SetSiteTheme,
                InstallUninstallTheme
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}