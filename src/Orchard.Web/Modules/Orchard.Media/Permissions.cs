using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Media {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMedia = new Permission { Description = "Managing Media Files", Name = "ManageMedia" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageMedia,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageMedia}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageMedia}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {ManageMedia}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}