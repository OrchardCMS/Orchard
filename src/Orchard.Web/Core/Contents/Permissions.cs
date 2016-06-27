using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class Permissions : IPermissionProvider {

        // Note - in code you should demand PublishContent, EditContent, or DeleteContent
        // Do not demand the "Own" variations - those are applied automatically when you demand the main ones

        public static readonly Permission PublishContent = new Permission { Description = "Publish or unpublish content for others", Name = "PublishContent" };
        public static readonly Permission PublishOwnContent = new Permission { Description = "Publish or unpublish own content", Name = "PublishOwnContent", ImpliedBy = new[] { PublishContent } };
        public static readonly Permission EditContent = new Permission { Description = "Edit content for others", Name = "EditContent", ImpliedBy = new[] { PublishContent } };
        public static readonly Permission EditOwnContent = new Permission { Description = "Edit own content", Name = "EditOwnContent", ImpliedBy = new[] { EditContent, PublishOwnContent } };
        public static readonly Permission DeleteContent = new Permission { Description = "Delete content for others", Name = "DeleteContent" };
        public static readonly Permission DeleteOwnContent = new Permission { Description = "Delete own content", Name = "DeleteOwnContent", ImpliedBy = new[] { DeleteContent } };
        public static readonly Permission ViewContent = new Permission { Description = "View all content", Name = "ViewContent", ImpliedBy = new[] { EditContent } };
        public static readonly Permission ViewOwnContent = new Permission { Description = "View own content", Name = "ViewOwnContent", ImpliedBy = new[] { ViewContent } };
        public static readonly Permission PreviewContent = new Permission { Description = "Preview content", Name = "PreviewContent", ImpliedBy = new[] { EditContent, PublishContent } };
        public static readonly Permission PreviewOwnContent = new Permission { Description = "Preview own content", Name = "PreviewOwnContent", ImpliedBy = new[] { PreviewContent } };


        public static readonly Permission MetaListContent = new Permission { ImpliedBy = new[] { EditOwnContent, PublishOwnContent, DeleteOwnContent } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                EditOwnContent,
                EditContent,
                PublishOwnContent,
                PublishContent,
                DeleteOwnContent,
                DeleteContent,
                ViewContent,
                ViewOwnContent,
                PreviewOwnContent,
                PreviewContent
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {PublishContent,EditContent,DeleteContent,PreviewContent}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishContent,EditContent,DeleteContent,PreviewContent}
                },
                new PermissionStereotype {
                    Name = "Moderator"
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishOwnContent,EditOwnContent,DeleteOwnContent,PreviewOwnContent}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditOwnContent,PreviewOwnContent}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {ViewContent}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {ViewContent}
                },
            };
        }

    }
}