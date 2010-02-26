using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Filters;
using Orchard.Security;

namespace Orchard.UI.Admin {
    public class AdminAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly IAuthorizer _authorizer;

        public AdminAuthorizationFilter(IAuthorizer authorizer) {
            _authorizer = authorizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext) {
            if (!IsAdmin(filterContext))
                return;

            if (!_authorizer.Authorize(StandardPermissions.AccessAdminPanel, T("Can't access the admin"))) {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        private static bool IsAdmin(AuthorizationContext filterContext) {
            if (string.Equals(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, "Admin",
                              StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }

            var adminAttributes = GetAdminAttributes(filterContext.ActionDescriptor);
            if (adminAttributes != null && adminAttributes.Any()) {
                return true;
            }
            return false;
        }

        private static IEnumerable<AdminAttribute> GetAdminAttributes(ActionDescriptor descriptor) {
            return descriptor.GetCustomAttributes(typeof(AdminAttribute), true)
                .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof(AdminAttribute), true))
                .OfType<AdminAttribute>();
        }
    }
}