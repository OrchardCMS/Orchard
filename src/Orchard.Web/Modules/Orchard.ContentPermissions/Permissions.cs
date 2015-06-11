using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ContentPermissions {
    public class Permissions : IPermissionProvider {

        // Note - in code you should demand GrantPermission
        // Do not demand the "Own" variation - it is applied automatically when you demand the main one

        public static readonly Permission GrantPermission = new Permission { Description = "Grant permissions for others", Name = "GrantPermission" };
        public static readonly Permission GrantOwnPermission = new Permission { Description = "Grant permission for own content", Name = "GrantOwnPermission", ImpliedBy = new[] { GrantPermission } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                GrantPermission,
                GrantOwnPermission
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {GrantPermission}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {GrantOwnPermission}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                }
            };
        }

    }
}