using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Themes;

namespace Orchard.OpenId.Controllers {
    [Themed]
    [OrchardFeature("Orchard.OpenId")]
    public class AccountController : Controller {

        public void LogOn(string openIdProvider) {
            if (string.IsNullOrWhiteSpace(openIdProvider))
                openIdProvider = OpenIdConnectAuthenticationDefaults.AuthenticationType;

            if (Request.IsAuthenticated) {
                Redirect(Url.Content("~/"));
                return;
            }

            var redirectUri = Url.Content(string.Concat("~", Constants.LogonCallbackUrl));

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, openIdProvider);
        }

        public RedirectResult LogOff(string openIdProvider) {
            if (string.IsNullOrWhiteSpace(openIdProvider))
                openIdProvider = OpenIdConnectAuthenticationDefaults.AuthenticationType;

            HttpContext.GetOwinContext().Authentication.SignOut(openIdProvider, CookieAuthenticationDefaults.AuthenticationType);

            return Redirect(Url.Content("~/"));
        }

        public ActionResult LogonCallback(string openIdProvider) {
            return Redirect(Url.Content("~/"));
        }

        public ActionResult AccessDenied() {
            return View();
        }

        public ActionResult Error() {
            return View();
        }

        [Authorize]
        public JsonResult Test() {
            var userName = HttpContext.GetOwinContext().Authentication.User.Identity.Name.Trim();

            return Json(new { Message = "Logged In as: " + userName }, JsonRequestBehavior.AllowGet);
        }
    }
}