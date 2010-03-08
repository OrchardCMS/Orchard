using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Tags {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageTags = new Permission { Description = "Manage tags", Name = "ManageTags" };
        public static readonly Permission CreateTag = new Permission { Description = "Create tag", Name = "CreateTag", ImpliedBy = new[] { ManageTags } };
        public static readonly Permission ApplyTag = new Permission { Description = "Applying a Tag", Name = "ApplyTag", ImpliedBy = new[] { ManageTags, CreateTag } };

        public string ModuleName {
            get {
                return "Tags";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageTags,
                CreateTag,
                ApplyTag,
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
                    Permissions = new[] {CreateTag, ApplyTag}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {ApplyTag}
                },
            };
        }

    }
}
