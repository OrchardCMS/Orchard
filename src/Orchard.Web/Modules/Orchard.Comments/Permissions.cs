using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Comments {
    public class Permissions : IPermissionProvider {
        public static readonly Permission AddComment = new Permission { Description = "Add comment", Name = "AddComment" };
        public static readonly Permission ManageComments = new Permission { Description = "Manage comments", Name = "ManageComments" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                AddComment,
                ManageComments,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageComments, AddComment}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {AddComment}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {AddComment}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {AddComment}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageComments, AddComment}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {AddComment}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {AddComment}
                },
            };
        }
    }
}
