using System.Web.Mvc;
using System.Web.Routing;
using Orchard.DisplayManagement;

namespace Orchard.Core.Navigation.Helpers {
    public static class AdminBreadcrumbsHtmlHelper {
        public static void AdminBreadcrumbs(this HtmlHelper htmlHelper, object context = null) {
            htmlHelper.AdminBreadcrumbs(Navigation.AdminBreadcrumbs.Name, context);
        }

        public static void AdminBreadcrumbs(this HtmlHelper htmlHelper, string menuName, object context = null) {
            var workContext = htmlHelper.ViewContext.GetWorkContext();
            var shapeFactory = (dynamic)workContext.Resolve<IShapeFactory>();
            var adminBreadcrumbs = shapeFactory.AdminBreadcrumbs(MenuName: menuName);
            workContext.Layout.Breadcrumbs.Add(adminBreadcrumbs);
            workContext.Layout.Breadcrumbs.Context = new RouteValueDictionary(context ?? new {});
        }
    }
}