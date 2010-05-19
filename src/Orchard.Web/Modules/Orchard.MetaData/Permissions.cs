using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.MetaData {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMetaData = new Permission { Description = "Manage MetaData", Name = "ManageMetaData" };//q: Should edit_MetaData be ManageMetaData?

        public static readonly Permission PublishOthersMetaDataPost = new Permission { Description = "Publish or unpublish MetaData post for others", Name = "PublishOthersMetaDataPost", ImpliedBy = new[] { ManageMetaData } };
        public static readonly Permission PublishMetaDataPost = new Permission { Description = "Publish or unpublish MetaData post", Name = "PublishMetaDataPost", ImpliedBy = new[] { PublishOthersMetaDataPost } };
        public static readonly Permission EditOthersMetaDataPost = new Permission { Description = "Edit any MetaData posts", Name = "EditOthersMetaDataPost", ImpliedBy = new[] { PublishOthersMetaDataPost } };
        public static readonly Permission EditMetaDataPost = new Permission { Description = "Edit own MetaData posts", Name = "EditMetaDataPost", ImpliedBy = new[] { EditOthersMetaDataPost, PublishMetaDataPost } };
        public static readonly Permission DeleteOthersMetaDataPost = new Permission { Description = "Delete MetaData post for others", Name = "DeleteOthersMetaDataPost", ImpliedBy = new[] { ManageMetaData } };
        public static readonly Permission DeleteMetaDataPost = new Permission { Description = "Delete MetaData post", Name = "DeleteMetaDataPost", ImpliedBy = new[] { DeleteOthersMetaDataPost } };

        public static readonly Permission MetaListOthersMetaData = new Permission { ImpliedBy = new[] { EditOthersMetaDataPost, PublishOthersMetaDataPost, DeleteOthersMetaDataPost } };
        public static readonly Permission MetaListMetaData = new Permission { ImpliedBy = new[] { EditMetaDataPost, PublishMetaDataPost, DeleteMetaDataPost } };

        public string ModuleName {
            get {
                return "MetaData";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageMetaData,
                EditMetaDataPost,
                EditOthersMetaDataPost,
                PublishMetaDataPost,
                PublishOthersMetaDataPost,
                DeleteMetaDataPost,
                DeleteOthersMetaDataPost,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageMetaData}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishOthersMetaDataPost,EditOthersMetaDataPost,DeleteOthersMetaDataPost}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishMetaDataPost,EditMetaDataPost,DeleteMetaDataPost}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditMetaDataPost}
                },
            };
        }

    }
}


