using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.Layouts.Filters {
    public class ControllerAccessorFilter : FilterProvider, IActionFilter {
        public const string CurrentControllerKey = "CurrentController";

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            filterContext.HttpContext.Items[CurrentControllerKey] = filterContext.Controller;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {}
    }
}