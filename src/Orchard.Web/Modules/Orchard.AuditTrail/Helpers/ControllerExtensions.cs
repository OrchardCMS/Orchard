using System;
using System.Web.Mvc;

namespace Orchard.AuditTrail.Helpers {
    public static class ControllerExtensions {
        public static RedirectResult RedirectReturn(this Controller controller, string returnUrl = null, Func<string> defaultReturnUrl = null) {
            return new RedirectResult(controller.Request.GetReturnUrl(returnUrl, defaultReturnUrl));
        }
    }
}