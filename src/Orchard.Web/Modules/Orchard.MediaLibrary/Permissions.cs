using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.MediaLibrary {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMediaContent = new Permission { Description = "Managing Media", Name = "ManageMediaContent" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageMediaContent,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageMediaContent}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageMediaContent}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {ManageMediaContent}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}