using System.Globalization;
using System.IO;
using System.Web.Mvc;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.Mvc.Filters {
    public class AdminFilter : FilterProvider, IActionFilter
    {
        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;

        public AdminFilter(IAuthorizer authorizer, ISiteService siteService)
        {
            _authorizer = authorizer;
            _siteService = siteService;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var siteUrl = _siteService.GetSiteSettings().SiteUrl;
            //todo: (heskew) get at the admin path in a less hacky way
            if (filterContext.HttpContext.Request.RawUrl.StartsWith(Path.Combine(siteUrl, "admin").Replace("\\", "/"), true, CultureInfo.InvariantCulture)
                && !_authorizer.Authorize(StandardPermissions.AccessAdminPanel, "Can't access the admin")) {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}