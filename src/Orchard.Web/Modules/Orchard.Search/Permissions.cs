using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Search {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSearchIndex = new Permission { Description = "Manage Search Index", Name = "ManageSearchIndex" };

        public string ModuleName {
            get {
                return "Search";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                                        ManageSearchIndex,
                                    };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                             new PermissionStereotype {
                                                          Name = "Administrator",
                                                          Permissions = new[] {ManageSearchIndex}
                                                      },
                         };
        }
    }
}