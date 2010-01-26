using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Blogs {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageBlogs = new Permission { Description = "Manage blogs", Name = "ManageBlogs" };//q: Should edit_blog be ManageBlogs?

        public static readonly Permission PublishOthersBlogPost = new Permission { Description = "Publish or unpublish blog post for others", Name = "PublishOthersBlogPost", ImpliedBy = new[] { ManageBlogs } };
        public static readonly Permission PublishBlogPost = new Permission { Description = "Publish or unpublish blog post", Name = "PublishBlogPost", ImpliedBy = new[] { PublishOthersBlogPost } };
        public static readonly Permission EditOthersBlogPost = new Permission { Description = "Edit any blog posts", Name = "EditOthersBlogPost", ImpliedBy = new[] { PublishOthersBlogPost } };
        public static readonly Permission EditBlogPost = new Permission { Description = "Edit own blog posts", Name = "EditBlogPost", ImpliedBy = new[] { EditOthersBlogPost, PublishBlogPost } };
        public static readonly Permission DeleteOthersBlogPost = new Permission { Description = "Delete blog post for others", Name = "DeleteOthersBlogPost", ImpliedBy = new[] { ManageBlogs } };
        public static readonly Permission DeleteBlogPost = new Permission { Description = "Delete blog post", Name = "DeleteBlogPost", ImpliedBy = new[] { DeleteOthersBlogPost } };

        public static readonly Permission MetaListOthersBlogs = new Permission { ImpliedBy = new[] { EditOthersBlogPost, PublishOthersBlogPost, DeleteOthersBlogPost } };
        public static readonly Permission MetaListBlogs = new Permission { ImpliedBy = new[] { EditBlogPost, PublishBlogPost, DeleteBlogPost } };

        public string PackageName {
            get {
                return "Blogs";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageBlogs,
                EditBlogPost,
                EditOthersBlogPost,
                PublishBlogPost,
                PublishOthersBlogPost,
                DeleteBlogPost,
                DeleteOthersBlogPost,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrators",
                    Permissions = new[] {ManageBlogs}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishOthersBlogPost,EditOthersBlogPost,DeleteOthersBlogPost}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    //Permissions = new[] {}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishBlogPost,EditBlogPost,DeleteBlogPost}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditBlogPost}
                },
            };
        }

    }
}


