using System.Web.Mvc;

namespace Orchard.Blogs.Extensions {
    public static class UrlHelperExtensions {
        public static string Blogs(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string Blog(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Item", "Blog", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogEdit(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Edit", "Blog", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPost(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return "#";
        }
    }
}