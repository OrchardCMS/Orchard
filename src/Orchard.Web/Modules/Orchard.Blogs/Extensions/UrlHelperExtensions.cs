using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Services;
using Orchard.Mvc.Extensions;

namespace Orchard.Blogs.Extensions {
    public static class UrlHelperExtensions {
        public static string Blogs(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Blog", new {area = "Orchard.Blogs"});
        }

        public static string BlogsForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "BlogAdmin", new {area = "Orchard.Blogs"});
        }

        public static string Blog(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.Action("Item", "Blog", new { blogPath = blogPart.As<IRoutableAspect>().Path, area = "Orchard.Blogs" });
        }

        public static string BlogLiveWriterManifest(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.AbsoluteAction(() => urlHelper.Action("Manifest", "LiveWriter", new { area = "XmlRpc" }));
        }

        public static string BlogRsd(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.AbsoluteAction(() => urlHelper.Action("Rsd", "RemoteBlogPublishing", new { blogPath = blogPart.As<IRoutableAspect>().Path, area = "Orchard.Blogs" }));
        }

        public static string BlogArchiveYear(this UrlHelper urlHelper, BlogPart blogPart, int year) {
            return urlHelper.Action("ListByArchive", "BlogPost", new { blogPath = blogPart.As<IRoutableAspect>().Path, archiveData = year.ToString(), area = "Orchard.Blogs" });
        }

        public static string BlogArchiveMonth(this UrlHelper urlHelper, BlogPart blogPart, int year, int month) {
            return urlHelper.Action("ListByArchive", "BlogPost", new { blogPath = blogPart.As<IRoutableAspect>().Path, archiveData = string.Format("{0}/{1}", year, month), area = "Orchard.Blogs" });
        }

        public static string BlogArchiveDay(this UrlHelper urlHelper, BlogPart blogPart, int year, int month, int day) {
            return urlHelper.Action("ListByArchive", "BlogPost", new { blogPath = blogPart.As<IRoutableAspect>().Path, archiveData = string.Format("{0}/{1}/{2}", year, month, day), area = "Orchard.Blogs" });
        }

        public static string BlogForAdmin(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.Action("Item", "BlogAdmin", new { blogId = blogPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "BlogAdmin", new { area = "Orchard.Blogs" });
        }

        public static string BlogEdit(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.Action("Edit", "BlogAdmin", new { blogId = blogPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogRemove(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.Action("Remove", "BlogAdmin", new { blogId = blogPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogPostCreate(this UrlHelper urlHelper, BlogPart blogPart) {
            return urlHelper.Action("Create", "BlogPostAdmin", new { blogId = blogPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogPost(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.Action("Item", "BlogPost", new { blogPath = blogPostPart.BlogPart.As<IRoutableAspect>().Path, postSlug = blogPostPart.As<IRoutableAspect>().GetEffectiveSlug(), area = "Orchard.Blogs" });
        }

        public static string BlogPostEdit(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.Action("Edit", "BlogPostAdmin", new { blogId = blogPostPart.BlogPart.Id, postId = blogPostPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogPostDelete(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.Action("Delete", "BlogPostAdmin", new { blogId = blogPostPart.BlogPart.Id, postId = blogPostPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogPostPublish(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.Action("Publish", "BlogPostAdmin", new { blogId = blogPostPart.BlogPart.Id, postId = blogPostPart.Id, area = "Orchard.Blogs" });
        }

        public static string BlogPostUnpublish(this UrlHelper urlHelper, BlogPostPart blogPostPart) {
            return urlHelper.Action("Unpublish", "BlogPostAdmin", new { blogId = blogPostPart.BlogPart.Id, postId = blogPostPart.Id, area = "Orchard.Blogs" });
        }
    }
}