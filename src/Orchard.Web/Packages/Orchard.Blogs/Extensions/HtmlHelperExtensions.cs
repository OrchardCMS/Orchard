using System.Web.Mvc;
using Orchard.Blogs.Models;
using Orchard.Mvc.Html;

namespace Orchard.Blogs.Extensions {
    public static class HtmlHelperExtensions {
        public static string Published(this HtmlHelper<BlogPost> htmlHelper) {
            return htmlHelper.Published(htmlHelper.ViewData.Model);
        }

        public static string Published(this HtmlHelper htmlHelper, BlogPost blogPost) {
            return htmlHelper.DateTime(blogPost.Published, "as a Draft");
        }
    }
}