using System.Web.Mvc;

namespace Orchard.Blogs.Extensions {
    public static class UrlHelperExtensions {
        public static string Blogs(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string BlogsForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "BlogAdmin", new {area = "Orchard.Blogs"});
        }

        public static string Blog(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Item", "Blog", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogArchiveYear(this UrlHelper urlHelper, string blogSlug, int year) {
            return urlHelper.Action("ListByArchive", "BlogPost", new { blogSlug, archiveData = year.ToString(), area = "Orchard.Blogs" });
        }

        public static string BlogArchiveMonth(this UrlHelper urlHelper, string blogSlug, int year, int month) {
            return urlHelper.Action("ListByArchive", "BlogPost", new { blogSlug, archiveData = string.Format("{0}/{1}", year, month), area = "Orchard.Blogs" });
        }

        public static string BlogArchiveDay(this UrlHelper urlHelper, string blogSlug, int year, int month, int day) {
            return urlHelper.Action("ListByArchive", "BlogPost", new {blogSlug, archiveData = string.Format("{0}/{1}/{2}", year, month, day), area = "Orchard.Blogs"});
        }

        public static string BlogForAdmin(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Item", "BlogAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "BlogAdmin", new {area = "Orchard.Blogs"});
        }

        public static string BlogEdit(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Edit", "BlogAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogDelete(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Delete", "BlogAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPost(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Item", "BlogPost", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostCreate(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Create", "BlogPostAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostEdit(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Edit", "BlogPostAdmin", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostDelete(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Delete", "BlogPostAdmin", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostPublish(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Publish", "BlogPostAdmin", new { blogSlug, postSlug, area = "Orchard.Blogs" });
        }
    }
}