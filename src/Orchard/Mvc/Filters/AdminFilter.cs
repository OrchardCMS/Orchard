using System.Web.Mvc;

namespace Orchard.Mvc.Filters {
    public class AdminFilter : FilterProvider, IActionFilter {
        public void OnActionExecuting(ActionExecutingContext filterContext) {
            //TODO: (erikpo) When Orchard needs to work from a virtual path, this check will need to be adjusted
            if (filterContext.HttpContext.Request.RawUrl.StartsWith("/Admin") && !filterContext.HttpContext.Request.IsAuthenticated)
                filterContext.Result = new HttpUnauthorizedResult();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
        }
    }
}