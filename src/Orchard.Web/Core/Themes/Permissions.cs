using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Themes {
    public class Permissions : IPermissionProvider {
        public static readonly Permission InstallUninstallTheme = new Permission { Description = "Installing or Uninstalling Themes", Name = "InstallUninstallTheme" };
        public static readonly Permission SetCurrentTheme = new Permission { Description = "Setting the Current Theme", Name = "SetCurrentTheme" };

        public string PackageName {
            get {
                return "Themes";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                SetCurrentTheme,
                InstallUninstallTheme
            };
        }
    }
}