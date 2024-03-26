using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Utility.Extensions;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Extensions {
    public static class ControllerExtensions {
        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, Func<ActionResult> invalidUrlBehavior) {
            if (!string.IsNullOrWhiteSpace(redirectUrl) && controller.Request.IsLocalUrl(redirectUrl)) {
                return RedirectWithTenantPrefix(redirectUrl, controller);
            }

            return invalidUrlBehavior != null ? invalidUrlBehavior() : null;
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl) {
            return RedirectLocal(controller, redirectUrl, (string)null);
        }

        public static ActionResult RedirectLocal(this Controller controller, string redirectUrl, string defaultUrl) {
            if (controller.Request.IsLocalUrl(redirectUrl)) {
                return RedirectWithTenantPrefix(redirectUrl, controller);
            }

            return RedirectWithTenantPrefix(defaultUrl ?? "~/", controller);
        }

        private static ActionResult RedirectWithTenantPrefix(string redirectUrl, Controller controller) {
            if (redirectUrl.StartsWith("~/")) {
                ShellSettings settings;
                var context = controller.ControllerContext.GetWorkContext();

                if (context != null &&
                    context.TryResolve<ShellSettings>(out settings) &&
                    !string.IsNullOrWhiteSpace(settings.RequestUrlPrefix)) {
                    redirectUrl = VirtualPathUtility.ToAbsolute(redirectUrl, controller.Request.ApplicationPath.TrimEnd('/') + "/" + settings.RequestUrlPrefix);
                }
            }

            return new RedirectResult(redirectUrl);
        }
    }
}
