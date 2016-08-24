using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Themes;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Orchard.Azure.Authentication.Services;
using Orchard.Logging;

namespace Orchard.Azure.Authentication.Controllers {
    [Themed]
    [OrchardSuppressDependency("Orchard.Users.Controllers.AccountController")]
    public class AccountController : Controller {
        public ILogger Logger { get; set; }

        private readonly IAzureGraphiApiService _graphiApiService;
        private readonly IAzureRolesPersistence _azureRolesPersistence;

        public AccountController(IAzureGraphiApiService graphiApiService, IAzureRolesPersistence azureRolesPersistence) {
            Logger = NullLogger.Instance;
            _graphiApiService = graphiApiService;
            _azureRolesPersistence = azureRolesPersistence;
        }
        public void LogOn() {
            if (Request.IsAuthenticated) {
                return; //TODO: redirect to home if we can?
            }

            var redirectUri = Url.Content("~/users/account/logoncallback");

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties {RedirectUri = redirectUri}, 
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void LogOff() {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType); //OpenID Connect sign-out request.
        }

        public ActionResult LogonCallback() {
            var userName = HttpContext.GetOwinContext().Authentication.User.Identity.Name.Trim();

            try {
                var groups = _graphiApiService.GetUserGroups(userName);
                _azureRolesPersistence.SyncAzureGroupsToOrchardRoles(userName, groups);
            }
            catch (Exception ex) {
                Logger.Error(ex.Message, ex);
            }

            return Redirect(Url.Content("~/"));
        }

        [AlwaysAccessible]
        public ActionResult AccessDenied() {
            return View();
        }
    }
}