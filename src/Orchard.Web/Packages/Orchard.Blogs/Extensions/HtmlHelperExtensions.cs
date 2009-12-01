using System.Web.Mvc;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Extensions {
    public static class HtmlHelperExtensions {
        public static string Published(this HtmlHelper<BlogPost> htmlHelper) {
            return htmlHelper.Published(htmlHelper.ViewData.Model);
        }

        public static string Published(this HtmlHelper htmlHelper, BlogPost blogPost) {
            //TODO: (erikpo) Relative time instead would be nice.
            return blogPost.Published.HasValue ? blogPost.Published.Value.ToString("{0:M d yyyy h:mm tt}") : "Draft";
        }
    }
}