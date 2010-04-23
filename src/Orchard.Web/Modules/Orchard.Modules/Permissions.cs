using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Modules {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageModules = new Permission { Description = "Manage Modules", Name = "ManageModules" };

        public string ModuleName {
            get { return "Modules"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {ManageModules};
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                             new PermissionStereotype {
                                                          Name = "Administrator",
                                                          Permissions = new[] {ManageModules}
                                                      }
                         };
        }
    }
}