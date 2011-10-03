using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Orchard.UI.Admin;

namespace Orchard.Users.Security {
    public class FrontEndFilter : FilterProvider, IAuthorizationFilter {
        private readonly IAuthorizer _authorizer;

        public FrontEndFilter(IAuthorizer authorizer) {
            _authorizer = authorizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext) {

            var isAuthPage = (filterContext.ActionDescriptor.ActionName == "LogOn"
                              || filterContext.ActionDescriptor.ActionName == "ChangePassword"
                              || filterContext.ActionDescriptor.ActionName == "AccessDenied"
                              || filterContext.ActionDescriptor.ActionName == "Register")
                             && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Account";

            if (!AdminFilter.IsApplied(filterContext.RequestContext) && !isAuthPage && !_authorizer.Authorize(StandardPermissions.AccessFrontEnd, T("Can't access this website"))) {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}
