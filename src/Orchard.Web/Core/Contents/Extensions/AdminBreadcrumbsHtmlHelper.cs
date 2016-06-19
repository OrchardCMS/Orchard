using System.Web.Mvc;
using Orchard.Core.Navigation.Helpers;

namespace Orchard.Core.Contents.Extensions {
    public static class AdminBreadcrumbsHtmlHelper {
        public static void AdminBreadcrumbs(this HtmlHelper htmlHelper, object context = null) {
            htmlHelper.AdminBreadcrumbs(Contents.AdminBreadcrumbs.Name, context);
        }
    }
}