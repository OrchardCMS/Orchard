using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace TinyMce {
    [OrchardFeature("TinyMce.Settings")]
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTinyMceSettings = new Permission { Description = "Manage TinyMCE settings", Name = "ManageTinyMceSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ManageTinyMceSettings;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            yield return new PermissionStereotype {
                Name = "Administrator",
                Permissions = new[] {
                    ManageTinyMceSettings
                }
            };
        }
    }
}