using System.Web.Mvc;
using Orchard.Core.Navigation.Helpers;

namespace Orchard.Blogs.Extensions {
    public static class AdminBreadcrumbsHtmlHelper {
        public static void AdminBreadcrumbs(this HtmlHelper htmlHelper, object context = null) {
            htmlHelper.AdminBreadcrumbs(Blogs.AdminBreadcrumbs.Name, context);
        }
    }
}