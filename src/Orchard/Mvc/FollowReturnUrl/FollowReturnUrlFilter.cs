using System;
using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.Mvc.FollowReturnUrl {
    public class FollowReturnUrlFilter : FilterProvider, IActionFilter {
        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var attributes =
                (FollowReturnUrlAttribute[])
                filterContext.ActionDescriptor.GetCustomAttributes(typeof (FollowReturnUrlAttribute), false);

            if (attributes.Length <= 0) {
                return;
            }

            var request = filterContext.HttpContext.Request;
            Uri returnUrl = null;
            try {
                returnUrl = new Uri(request.QueryString["ReturnUrl"]);
            }
            catch {
                try {
                    returnUrl =
                        new Uri(string.Format("{0}://{1}{2}{3}", request.Url.Scheme, request.Url.Host,
                                              request.Url.Port != 80 ? ":" + request.Url.Port : "",
                                              request.QueryString["ReturnUrl"]));
                }
                catch {}
            }

            if (returnUrl != null &&
                returnUrl.Scheme == request.Url.Scheme &&
                returnUrl.Port == request.Url.Port &&
                returnUrl.Host == request.Url.Host) {
                filterContext.Result = new RedirectResult(returnUrl.ToString());
            }
        }
    }
}