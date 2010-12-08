using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Modules {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageFeatures = new Permission {Description = "Manage Features", Name = "ManageFeatures" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {ManageFeatures};
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                             new PermissionStereotype {
                                                          Name = "Administrator",
                                                          Permissions = new[] {ManageFeatures}
                                                      }
                         };
        }
    }
}