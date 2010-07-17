using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class Permissions : IPermissionProvider {
        public static readonly Permission PublishOthersContent = new Permission { Description = "Publish or unpublish content for others", Name = "PublishOthersContent" };
        public static readonly Permission PublishContent = new Permission { Description = "Publish or unpublish content", Name = "PublishContent", ImpliedBy = new[] { PublishOthersContent } };
        public static readonly Permission EditOthersContent = new Permission { Description = "Edit content for others", Name = "EditOthersContent", ImpliedBy = new[] { PublishOthersContent } };
        public static readonly Permission EditContent = new Permission { Description = "Edit content", Name = "EditContent", ImpliedBy = new[] { EditOthersContent, PublishContent } };
        public static readonly Permission DeleteOthersContent = new Permission { Description = "Delete content for others", Name = "DeleteOthersContent" };
        public static readonly Permission DeleteContent = new Permission { Description = "Delete content", Name = "DeleteContent", ImpliedBy = new[] { DeleteOthersContent } };

        public static readonly Permission MetaListContent = new Permission { ImpliedBy = new[] { EditContent, PublishContent, DeleteContent } };

        public string ModuleName {
            get {
                return "Content";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                EditContent,
                EditOthersContent,
                PublishContent,
                PublishOthersContent,
                DeleteContent,
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
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishContent,EditContent,DeleteContent}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditContent}
                },
            };
        }

    }
}