using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Security;

namespace Orchard.Users.Services {
    public class SecurityFilter : FilterProvider, IAuthorizationFilter {
        private readonly IAuthenticationService _authenticationService;

        public SecurityFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext) {

            var accessToAuthorizedUserOnly = filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() || filterContext.ActionDescriptor.ControllerDescriptor.ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();

            ////When user has logged out from a different browser, we have to invalidate all other browser sessions too.
            ////_authenticationService.GetAuthenticatedUser() is null if the user has logged out from a different browser.
            if (accessToAuthorizedUserOnly && filterContext.RequestContext.HttpContext.Request.IsAuthenticated && _authenticationService.GetAuthenticatedUser() == null) {
                _authenticationService.SignOut();
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}