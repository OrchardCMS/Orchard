using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Blogs {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageBlogs = new Permission { Description = "Edit blog properties", Name = "ManageBlogs" };//q: Should edit_blog be ManageBlogs?

        public static readonly Permission EditBlogPost = new Permission { Description = "Edit own blog posts", Name = "EditBlogPost" };
        public static readonly Permission EditOthersBlogPost = new Permission { Description = "Edit any blog posts", Name = "EditOthersBlogPost" };
        public static readonly Permission PublishBlogPost = new Permission { Description = "Publish or unpublish blog post", Name = "PublishBlogPost" };
        public static readonly Permission PublishOthersBlogPost = new Permission { Description = "Publish or unpublish blog post for others", Name = "PublishOthersBlogPost" };
        public static readonly Permission DeleteBlogPost = new Permission { Description = "Delete blog post", Name = "DeleteBlogPost" };
        public static readonly Permission DeleteOthersBlogPost = new Permission { Description = "Delete blog post for others", Name = "DeleteOthersBlogPost" };
        

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
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}


