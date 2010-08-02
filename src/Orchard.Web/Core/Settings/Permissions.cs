using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Settings {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSettings = new Permission { Name = "ManageSettings", Description = "Manage site settings" };
        public static readonly Permission ChangeSuperuser = new Permission { Name = "ChangeSuperuser", Description = "Change the superuser for the site" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                 ManageSettings,
                 ChangeSuperuser,
             };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSettings}
                }
            };
        }
    }
}
