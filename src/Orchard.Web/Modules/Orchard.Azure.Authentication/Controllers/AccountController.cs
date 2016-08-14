using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Services;
using Orchard.Themes;
using Orchard.Users.Events;
using Orchard.Azure.Authentication.Models;
using Orchard.Azure.Authentication.Services;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Azure.Authentication.Controllers
{
    [Themed]
    [OrchardSuppressDependency("Orchard.Users.Controllers.AccountController")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IAzureAuthorizer _azureAuthorizer;
        private readonly IRepository<AzureActiveDirectorySyncPartRecord> _azureActiveDirectorySyncPartRepository;
        private readonly IClock _clock;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IOrchardServices _orchardServices;

        public AccountController(IAuthenticationService authenticationService, IMembershipService membershipService,
            IAzureAuthorizer azureAuthorizer, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository,
            IClock clock, IUserEventHandler _userEventHandler, IOrchardServices orchardServices)
        {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _azureAuthorizer = azureAuthorizer;
            _azureActiveDirectorySyncPartRepository = azureActiveDirectorySyncPartRepository;
            _clock = clock;
            _orchardServices = orchardServices;
        }

        public void LogOn()
        {
            if (Request.IsAuthenticated)
            {
                return;
            }

            var redirectUri = Url.Content("~/");

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
              OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType); //OpenID Connect sign-out request.
        }

        public ActionResult LogOnCallBack()
        {
            string userName = HttpContext.User.Identity.Name;
            if (false == Request.IsAuthenticated)
            {
                //Note: this method call will sync Orchard Role membership with with Azure AD group Membership if required.
                _azureAuthorizer.GetOrCreateOrchardUser(userName, _azureActiveDirectorySyncPartRepository, _clock);
            }

            //Fire the Orchard Login Events
            _userEventHandler.LoggingIn(userName, "not applicable");
            IUser user = _membershipService.GetUser(userName);
            _userEventHandler.LoggedIn(user);

            return new RedirectResult("~/");
        }

        [AlwaysAccessible]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}