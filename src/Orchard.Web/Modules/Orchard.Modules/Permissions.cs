using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Modules {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageModules = new Permission { Description = "Manage Modules", Name = "ManageModules" };
        public static readonly Permission ManageFeatures = new Permission { Description = "Manage Features", Name = "ManageFeatures", ImpliedBy = new[] {ManageModules}};

        public string ModuleName {
            get { return "Modules"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {ManageModules, ManageFeatures};
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