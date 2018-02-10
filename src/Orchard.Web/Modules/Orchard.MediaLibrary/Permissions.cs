using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.MediaLibrary {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMediaContent = new Permission { Description = "Manage Media", Name = "ManageMediaContent" };
        public static readonly Permission ImportMediaContent = new Permission { Description = "Import All Media", Name = "ImportMedia", ImpliedBy = new[] { ManageMediaContent } };
        public static readonly Permission EditMediaContent = new Permission { Description = "Edit All Media", Name = "EditMedia", ImpliedBy = new[] { ManageMediaContent } };
        public static readonly Permission DeleteMediaContent = new Permission { Description = "Delete All Media", Name = "DeleteMedia", ImpliedBy = new[] { ManageMediaContent } };
        public static readonly Permission SelectMediaContent = new Permission { Description = "Select All Media", Name = "SelectMedia", ImpliedBy = new[] { ManageMediaContent, ImportMediaContent, EditMediaContent, DeleteMediaContent } };
        public static readonly Permission ManageOwnMedia = new Permission { Description = "Manage Own Media", Name = "ManageOwnMedia", ImpliedBy = new[] { ManageMediaContent, SelectMediaContent, ImportMediaContent, EditMediaContent, DeleteMediaContent } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageMediaContent,
                ImportMediaContent,
                EditMediaContent,
                DeleteMediaContent,
                SelectMediaContent,
                ManageOwnMedia,
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
                    Permissions = new[] {ManageOwnMedia}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {ManageOwnMedia}
                },
            };
        }

    }
}