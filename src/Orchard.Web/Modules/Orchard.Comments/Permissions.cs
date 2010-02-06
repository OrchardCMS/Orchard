using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Comments {
    public class Permissions : IPermissionProvider {
        public static readonly Permission AddComment = new Permission { Description = "Add comment", Name = "AddComment" };
        public static readonly Permission EnableComment = new Permission { Description = "Enabling Comments on content items", Name = "EnableComment" };//refactoring
        public static readonly Permission CloseComment = new Permission { Description = "Closing Comments", Name = "CloseComment" };//refactoring
        public static readonly Permission CloseCommentOnOwnItems = new Permission { Description = "Closing Comments on own items", Name = "CloseCommentOnOwnItems" };//refactoring
        public static readonly Permission ManageComments = new Permission { Description = "Manage comments", Name = "ManageComments" };
        public static readonly Permission ManageOthersComments = new Permission { Description = "Manage comments for others", Name = "ManageOthersComments" };

        public string ModuleName {
            get {
                return "Comments";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                AddComment,
                EnableComment,
                CloseComment,
                CloseCommentOnOwnItems,
                ManageComments,
                ManageOthersComments
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrators",
                    Permissions = new[] {ManageComments}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {AddComment}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Author",
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    //Permissions = new[] {}
                },
            };
        }
    }
}
