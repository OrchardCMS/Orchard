using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Indexing {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSearchIndex = new Permission { Description = "Manage Search Index", Name = "ManageSearchIndex" };

        public string ModuleName {
            get {
                return "Indexing";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ManageSearchIndex };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                             new PermissionStereotype {
                                                          Name = "Administrator",
                                                          Permissions = new [] { ManageSearchIndex }
                                                      },
                         };
        }
    }
}