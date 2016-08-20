using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Themes;
using System.Web;
using System.Web.Mvc;
using Orchard.Azure.Authentication.Services;

namespace Orchard.Azure.Authentication.Controllers {
    [Themed]
    [OrchardSuppressDependency("Orchard.Users.Controllers.AccountController")]
    public class AccountController : Controller {
        private readonly IAzureGraphiApiService _graphiApiService;

        public AccountController(IAzureGraphiApiService graphiApiService) {
            _graphiApiService = graphiApiService;
        }
        public void LogOn() {
            if (Request.IsAuthenticated) {
                return; //TODO: redirect to home
            }

            var redirectUri = Url.Content("~/");

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties {RedirectUri = redirectUri}, 
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void LogOff() {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType); //OpenID Connect sign-out request.
        }

        public void Test() {
            var userName = HttpContext.GetOwinContext().Authentication.User.Identity.Name.Trim();

            var user = _graphiApiService.GetAzureUser(userName);
        }

        [AlwaysAccessible]
        public ActionResult AccessDenied() {
            return View();
        }
    }
}