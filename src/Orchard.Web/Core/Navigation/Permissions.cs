using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Navigation {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMainMenu = new Permission { Name = "ManageMainMenu", Description = "Manage main menu" };

        public string ModuleName {
            get { return "Navigation"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageMainMenu
             };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrators",
                    Permissions = new[] {ManageMainMenu}
                }
            };
        }
    }
}
