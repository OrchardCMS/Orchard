using System.Globalization;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.Core.Common.Mvc.Filters {
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
            //todo: (heskew) get at the SiteUrl the "right" way. or is this the right way :|
            var siteUrl = _siteService.GetSiteSettings().ContentItem.As<SiteSettings>().Record.SiteUrl;

            if (filterContext.HttpContext.Request.RawUrl.StartsWith(Path.Combine(siteUrl, "admin"), true, CultureInfo.InvariantCulture)
                && !_authorizer.Authorize(Permissions.AccessAdmin, "Can't access the admin")) {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}