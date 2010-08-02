using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Mvc.Extensions;

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

        public static string BlogLiveWriterManifest(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.AbsoluteAction(() => urlHelper.Action("LiveWriterManifest", "Blog", new { blogSlug, area = "Orchard.Blogs" }));
        }

        public static string BlogRsd(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.AbsoluteAction(() => urlHelper.Action("Rsd", "Blog", new { blogSlug, area = "Orchard.Blogs" }));
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

        public static string BlogRemove(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Remove", "BlogAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPost(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.BlogPost(blogPostPart.BlogPart.Slug, blogPostPart.Slug);
        }

        public static string BlogPost(this UrlHelper urlHelper, string blogSlug, string postSlug) {
            return urlHelper.Action("Item", "BlogPost", new {blogSlug, postSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostCreate(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.BlogPostCreate(blogPart.Slug);
        }

        public static string BlogPostCreate(this UrlHelper urlHelper, string blogSlug) {
            return urlHelper.Action("Create", "BlogPostAdmin", new {blogSlug, area = "Orchard.Blogs"});
        }

        public static string BlogPostEdit(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.BlogPostEdit(blogPostPart.BlogPart.Slug, blogPostPart.Id);
        }

        public static string BlogPostEdit(this UrlHelper urlHelper, string blogSlug, int postId) {
            return urlHelper.Action("Edit", "BlogPostAdmin", new {blogSlug, postId, area = "Orchard.Blogs"});
        }

        public static string BlogPostDelete(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.BlogPostDelete(blogPostPart.BlogPart.Slug, blogPostPart.Id);
        }

        public static string BlogPostDelete(this UrlHelper urlHelper, string blogSlug, int postId) {
            return urlHelper.Action("Delete", "BlogPostAdmin", new {blogSlug, postId, area = "Orchard.Blogs"});
        }

        public static string BlogPostPublish(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.BlogPostPublish(blogPostPart.BlogPart.Slug, blogPostPart.Id);
        }

        public static string BlogPostPublish(this UrlHelper urlHelper, string blogSlug, int postId) {
            return urlHelper.Action("Publish", "BlogPostAdmin", new { blogSlug, postId, area = "Orchard.Blogs" });
        }

        public static string BlogPostUnpublish(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.BlogPostUnpublish(blogPostPart.BlogPart.Slug, blogPostPart.Id);
        }

        public static string BlogPostUnpublish(this UrlHelper urlHelper, string blogSlug, int postId) {
            return urlHelper.Action("Unpublish", "BlogPostAdmin", new { blogSlug, postId, area = "Orchard.Blogs" });
        }
    }
}