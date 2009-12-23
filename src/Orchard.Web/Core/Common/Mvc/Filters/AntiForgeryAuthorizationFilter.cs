using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Mvc.Filters;
using Orchard.Settings;

namespace Orchard.Core.Common.Mvc.Filters {
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly ISiteService _siteService;

        public AntiForgeryAuthorizationFilter(ISiteService siteService) {
            _siteService = siteService;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            if (!(filterContext.HttpContext.Request.HttpMethod == "POST" && filterContext.RequestContext.HttpContext.Request.IsAuthenticated))
                return;

            var siteSalt = _siteService.GetSiteSettings().ContentItem.As<SiteSettings>().Record.SiteSalt;
            ValidateAntiForgeryTokenAttribute validator = new ValidateAntiForgeryTokenAttribute { Salt = siteSalt };

            validator.OnAuthorization(filterContext);
        }
    }
}