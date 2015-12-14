using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Tags {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTags = new Permission { Description = "Manage tags", Name = "ManageTags" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageTags,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageTags}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageTags}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageTags}
                },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}
