using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Settings {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSettings = new Permission { Description = "Manage Settings", Name = "ManageSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSettings}
                },
            };
        }

    }
}