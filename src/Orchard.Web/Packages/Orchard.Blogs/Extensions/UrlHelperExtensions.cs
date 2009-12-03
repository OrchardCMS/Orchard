using System.Web.Mvc;

namespace Orchard.Blogs.Extensions {
    public static class UrlHelperExtensions {
        public static string Blogs(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string BlogsForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("ListForAdmin", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string Blog(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Item", "Blog", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string BlogEdit(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Edit", "Blog", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPost(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Item", "BlogPost", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostCreate(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Create", "BlogPost", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostEdit(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Edit", "BlogPost", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }
    }
}