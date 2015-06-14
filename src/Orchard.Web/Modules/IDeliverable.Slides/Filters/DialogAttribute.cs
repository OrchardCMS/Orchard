using System;
using System.Web.Mvc;
using Orchard;

namespace IDeliverable.Slides.Filters
{
    public class DialogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var dialogMode = filterContext.HttpContext.Request.QueryString["dialog"];
            var useDialogMode = dialogMode != null && !String.Equals(dialogMode, "false", StringComparison.OrdinalIgnoreCase);

            if (useDialogMode)
            {
                var workContext = filterContext.GetWorkContext();
                workContext.Layout.Metadata.Alternates.Add("Layout__Dialog");
            }
        }
    }
}