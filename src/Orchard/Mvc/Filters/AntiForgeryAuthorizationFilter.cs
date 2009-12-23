using System.Web.Mvc;
using Orchard.Settings;

namespace Orchard.Mvc.Filters {
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly ISiteService _siteService;

        public AntiForgeryAuthorizationFilter(ISiteService siteService) {
            _siteService = siteService;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            if (!(filterContext.HttpContext.Request.HttpMethod == "POST" && filterContext.RequestContext.HttpContext.Request.IsAuthenticated))
                return;

            var siteSalt = _siteService.GetSiteSettings().SiteSalt;
            ValidateAntiForgeryTokenAttribute validator = new ValidateAntiForgeryTokenAttribute { Salt = siteSalt };

            validator.OnAuthorization(filterContext);
        }
    }
}