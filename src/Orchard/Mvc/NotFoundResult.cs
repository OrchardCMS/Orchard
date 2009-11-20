using System.Web.Mvc;

namespace Orchard.Mvc.Results {
    public class NotFoundResult : ViewResult {
        public override void ExecuteResult(ControllerContext context) {
            ViewName = "NotFound";

            ViewData = context.Controller.ViewData;
            TempData = context.Controller.TempData;

            base.ExecuteResult(context);

            context.HttpContext.Response.StatusDescription = "File Not Found";
            context.HttpContext.Response.StatusCode = 404;
        }
    }
}