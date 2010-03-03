using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Mvc.Html;

namespace Orchard.Blogs.Extensions {
    public static class HtmlHelperExtensions {
        public static string PublishedState(this HtmlHelper<BlogPost> htmlHelper) {
            return htmlHelper.PublishedState(htmlHelper.ViewData.Model);
        }

        public static string PublishedState(this HtmlHelper htmlHelper, BlogPost blogPost) {
            return htmlHelper.DateTime(blogPost.As<ICommonAspect>().VersionPublishedUtc, "Draft");
        }

        public static string PublishedWhen(this HtmlHelper<BlogPost> htmlHelper) {
            return htmlHelper.PublishedWhen(htmlHelper.ViewData.Model);
        }

        public static string PublishedWhen(this HtmlHelper htmlHelper, BlogPost blogPost) {
            return htmlHelper.DateTimeRelative(blogPost.As<ICommonAspect>().VersionPublishedUtc, "as a Draft");
        }
    }
}