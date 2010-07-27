using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Indexing {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSearchIndex = new Permission { Description = "Manage Search Index", Name = "ManageSearchIndex" };

        public virtual Feature Feature { get; set; }

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