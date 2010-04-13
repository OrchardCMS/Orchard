using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Extensions {
    public static class ControllerExtensions {
        public static RedirectResult ReturnUrlRedirect(this Controller controller) {
            var request = controller.HttpContext.Request;
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
                catch { }
            }

            if (returnUrl != null &&
                returnUrl.Scheme == request.Url.Scheme &&
                returnUrl.Port == request.Url.Port &&
                returnUrl.Host == request.Url.Host) {
                return new RedirectResult(returnUrl.ToString());
            }
            return new RedirectResult("~/");
        }
    }
}
