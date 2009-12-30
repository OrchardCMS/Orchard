using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.Mvc.Filters {
    [UsedImplicitly]
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly ISiteService _siteService;
        private readonly IAuthenticationService _authenticationService;

        public AntiForgeryAuthorizationFilter(ISiteService siteService, IAuthenticationService authenticationService) {
            _siteService = siteService;
            _authenticationService = authenticationService;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            // not a post: no work to do
            if (filterContext.HttpContext.Request.HttpMethod != "POST")
                return;
            
            // not logged in: no attack vector
            if (_authenticationService.GetAuthenticatedUser() == null)
                return;
            
            var siteSalt = _siteService.GetSiteSettings().SiteSalt;
            var validator = new ValidateAntiForgeryTokenAttribute { Salt = siteSalt };
            validator.OnAuthorization(filterContext);
        }
    }
}
