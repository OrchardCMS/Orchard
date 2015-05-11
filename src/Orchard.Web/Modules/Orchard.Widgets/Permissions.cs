using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Orchard.Widgets {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ChangeWidgetsPositionAndLayer = new Permission { Description = "Change Widgets Posistion And Layer", Name = "ChangeWidgetsPositionAndLayer" };
        public static readonly Permission ManageWidgets = new Permission { Description = "Managing Widgets", Name = "ManageWidgets", ImpliedBy = new[] { ChangeWidgetsPositionAndLayer } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageWidgets,
                ChangeWidgetsPositionAndLayer
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageWidgets , ChangeWidgetsPositionAndLayer}
                },
            };
        }

    }
}