using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class Permissions : IPermissionProvider {
        public static readonly Permission PublishOthersContent = new Permission { Description = "Publish or unpublish content for others", Name = "PublishOthersContent" };
        public static readonly Permission PublishOwnContent = new Permission { Description = "Publish or unpublish own content", Name = "PublishOwnContent", ImpliedBy = new[] { PublishOthersContent } };
        public static readonly Permission EditOthersContent = new Permission { Description = "Edit content for others", Name = "EditOthersContent", ImpliedBy = new[] { PublishOthersContent } };
        public static readonly Permission EditOwnContent = new Permission { Description = "Edit own content", Name = "EditOwnContent", ImpliedBy = new[] { EditOthersContent, PublishOwnContent } };
        public static readonly Permission DeleteOthersContent = new Permission { Description = "Delete content for others", Name = "DeleteOthersContent" };
        public static readonly Permission DeleteOwnContent = new Permission { Description = "Delete own content", Name = "DeleteOwnContent", ImpliedBy = new[] { DeleteOthersContent } };

        public static readonly Permission MetaListContent = new Permission { ImpliedBy = new[] { EditOwnContent, PublishOwnContent, DeleteOwnContent } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new [] {
                EditOwnContent,
                EditOthersContent,
                PublishOwnContent,
                PublishOthersContent,
                DeleteOwnContent,
                DeleteOthersContent,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {PublishOthersContent,EditOthersContent,DeleteOthersContent}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishOthersContent,EditOthersContent,DeleteOthersContent}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishOwnContent,EditOwnContent,DeleteOwnContent}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditOwnContent}
                },
            };
        }

    }
}