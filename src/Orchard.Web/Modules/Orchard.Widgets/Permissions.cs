using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Widgets {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageWidgets = new Permission { Description = "Managing Widgets", Name = "ManageWidgets" };
        public static readonly Permission EditWidgetWrapper = new Permission { Description = "Edit Widget Wrapper in front-end", Name = "EditWidgetWrapper" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageWidgets,EditWidgetWrapper
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageWidgets, EditWidgetWrapper }
                },
            };
        }

    }
}