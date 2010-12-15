using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Blogs {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageBlogs = new Permission { Description = "Manage blogs", Name = "ManageBlogs" };

        public static readonly Permission PublishOthersBlogPost = new Permission { Description = "Publish or unpublish blog post for others", Name = "PublishOthersBlogPost", ImpliedBy = new[] { ManageBlogs } };
        public static readonly Permission PublishOwnBlogPost = new Permission { Description = "Publish or unpublish own blog post", Name = "PublishOwnBlogPost", ImpliedBy = new[] { PublishOthersBlogPost } };
        public static readonly Permission EditOthersBlogPost = new Permission { Description = "Edit any blog posts", Name = "EditOthersBlogPost", ImpliedBy = new[] { PublishOthersBlogPost } };
        public static readonly Permission EditOwnBlogPost = new Permission { Description = "Edit own blog posts", Name = "EditOwnBlogPost", ImpliedBy = new[] { EditOthersBlogPost, PublishOwnBlogPost } };
        public static readonly Permission DeleteOthersBlogPost = new Permission { Description = "Delete blog post for others", Name = "DeleteOthersBlogPost", ImpliedBy = new[] { ManageBlogs } };
        public static readonly Permission DeleteOwnBlogPost = new Permission { Description = "Delete own blog post", Name = "DeleteOwnBlogPost", ImpliedBy = new[] { DeleteOthersBlogPost } };

        public static readonly Permission MetaListOthersBlogs = new Permission { ImpliedBy = new[] { EditOthersBlogPost, PublishOthersBlogPost, DeleteOthersBlogPost } };
        public static readonly Permission MetaListOwnBlogs = new Permission { ImpliedBy = new[] { EditOwnBlogPost, PublishOwnBlogPost, DeleteOwnBlogPost } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageBlogs,
                EditOwnBlogPost,
                EditOthersBlogPost,
                PublishOwnBlogPost,
                PublishOthersBlogPost,
                DeleteOwnBlogPost,
                DeleteOthersBlogPost,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageBlogs}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {PublishOthersBlogPost,EditOthersBlogPost,DeleteOthersBlogPost}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishOwnBlogPost,EditOwnBlogPost,DeleteOwnBlogPost}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditOwnBlogPost}
                },
            };
        }

    }
}


