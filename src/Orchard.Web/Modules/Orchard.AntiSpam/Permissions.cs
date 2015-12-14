using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.AntiSpam {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageAntiSpam = new Permission { Description = "Manage anti-spam", Name = "ManageAntiSpam" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageAntiSpam,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageAntiSpam}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageAntiSpam}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageAntiSpam}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new Permission[0]
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new Permission[0] 
                },
            };
        }

    }
}
