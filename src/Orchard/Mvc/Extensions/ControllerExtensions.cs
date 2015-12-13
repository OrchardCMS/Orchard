using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Extensions {
    public static class ControllerExtensions {
        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, Func<ActionResult> invalidUrlBehavior) {
            if (!string.IsNullOrWhiteSpace(redirectUrl) && controller.Request.IsLocalUrl(redirectUrl)) {
                return new RedirectResult(redirectUrl);
            }
            return invalidUrlBehavior != null ? invalidUrlBehavior() : null;
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl) {
            return RedirectLocal(controller, redirectUrl, (string)null);
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, string defaultUrl) {
            if (controller.Request.IsLocalUrl(redirectUrl)) {
                return new RedirectResult(redirectUrl);
            }

            return new RedirectResult(defaultUrl ?? "~/");
        }
    }
}
