using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Blogs.Extensions {
    public static class HtmlHelperExtensions {
        public static LocalizedString PublishedState(this HtmlHelper<BlogPost> htmlHelper, Localizer T) {
            return htmlHelper.PublishedState(htmlHelper.ViewData.Model, T);
        }

        public static LocalizedString PublishedState(this HtmlHelper htmlHelper, BlogPost blogPost, Localizer T) {
            return htmlHelper.DateTime(blogPost.As<ICommonAspect>().VersionPublishedUtc, T("Draft"));
        }

        public static LocalizedString PublishedWhen(this HtmlHelper<BlogPost> htmlHelper, Localizer T) {
            return htmlHelper.PublishedWhen(htmlHelper.ViewData.Model, T);
        }

        public static LocalizedString PublishedWhen(this HtmlHelper htmlHelper, BlogPost blogPost, Localizer T) {
            return htmlHelper.DateTimeRelative(blogPost.As<ICommonAspect>().VersionPublishedUtc, T("as a Draft"), T);
        }
    }
}