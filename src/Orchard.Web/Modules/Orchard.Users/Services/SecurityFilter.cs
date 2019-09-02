using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Security;

namespace Orchard.Users.Services {
    public class SecurityFilter : FilterProvider, IAuthorizationFilter {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipValidationService _membershipValidationService;

        public SecurityFilter(IAuthenticationService authenticationService, IMembershipValidationService membershipValidationService) {
            _authenticationService = authenticationService;
            _membershipValidationService = membershipValidationService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext) {
            var accessToAuthorizedUserOnly =
                filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() ||
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();

            // When user has logged out from a different browser, we have to invalidate all other browser sessions too.
            // _membershipValidationService.CanAuthenticateWithCookie returns false if the user has logged out from a different browser.
            if (accessToAuthorizedUserOnly && filterContext.RequestContext.HttpContext.Request.IsAuthenticated &&
                !_membershipValidationService.CanAuthenticateWithCookie(_authenticationService.GetAuthenticatedUser())) {
                _authenticationService.SignOut();
                
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}
