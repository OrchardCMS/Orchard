using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Extensions {
    public static class ControllerExtensions {
        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, Func<ActionResult> invalidUrlBehavior) {
            if (!string.IsNullOrWhiteSpace(redirectUrl) && controller.Url.IsLocalUrl(redirectUrl)) {
                return new RedirectResult(redirectUrl);
            }
            return invalidUrlBehavior != null ? invalidUrlBehavior() : null;
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl) {
            return RedirectLocal(controller, redirectUrl, (string)null);
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, string defaultUrl) {
            if (!string.IsNullOrWhiteSpace(redirectUrl) 
                && controller.Url.IsLocalUrl(redirectUrl)
                && !redirectUrl.StartsWith("//")
                && !redirectUrl.StartsWith("/\\")) {
                
                
                return new RedirectResult(redirectUrl);
            }
            return new RedirectResult(defaultUrl ?? "~/");
        }
    }
}
