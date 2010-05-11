using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Extensions {
    public static class ControllerExtensions {
        public static RedirectResult ReturnUrlRedirect(this Controller controller) {
            string returnUrl = controller.Request.QueryString["ReturnUrl"];

            // prevents phishing attacks by using only relative urls
            if(!returnUrl.StartsWith("/")) {
                return new RedirectResult("~/");
            }

            return new RedirectResult(returnUrl);
        }
    }
}
