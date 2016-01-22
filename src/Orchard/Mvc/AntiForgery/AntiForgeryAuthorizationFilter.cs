using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Security;

namespace Orchard.Mvc.AntiForgery {
    [UsedImplicitly]
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly IAuthenticationService _authenticationService;
        private readonly IExtensionManager _extensionManager;

        public AntiForgeryAuthorizationFilter(IAuthenticationService authenticationService, IExtensionManager extensionManager) {
            _authenticationService = authenticationService;
            _extensionManager = extensionManager;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            var request = filterContext.HttpContext.Request;

            // If the action is adorned with the ValidateAntiForgeryTokenOrchard(false) attribute
            // then we do not validate under any circumstances. This allows validation to be
            // disabled for particular actions when a module has AntiForgery enabled.
            if (ShouldNotValidate(filterContext)) {
                return;
            }

            // We don't perform CSRF checks on un-authenticated requests because 
            // generally the whole point of CSRF checks is to ensure data is not submitted to
            // authenticated sessions from untrusted sources.
            if (_authenticationService.GetAuthenticatedUser() == null) {
                return;
            }

            // If the action is adorned with the ValidateAntiForgeryTokenOrchard(true) attribute then
            // we always validate regardless of the module's AntiForgery setting or the http method
            // of the request. Otherwise we'll only validate if anti-forgery is enabled for the module
            // and the request is not made via a "safe" method (eg. GET and other "safe" methods, as
            // defined by 9.1.1 Safe Methods, HTTP 1.1, RFC 2616).
            if (!ShouldValidate(filterContext)) {
                if (IsSafeMethod(request.HttpMethod) || !IsAntiForgeryProtectionEnabled(filterContext))
                    return;
            }

            // If the request header or the request query string contains the
            // verification token, then use this to validate the request.
            string requestVerificationToken = request.Headers.Get("X-Request-Verification-Token") ?? request.QueryString["__RequestVerificationToken"];
            if (!string.IsNullOrWhiteSpace(requestVerificationToken)) {
                var cookie = request.Cookies[System.Web.Helpers.AntiForgeryConfig.CookieName];
                System.Web.Helpers.AntiForgery.Validate(cookie != null ? cookie.Value : null, requestVerificationToken);
            }
            else {
                var validator = new ValidateAntiForgeryTokenAttribute();
                validator.OnAuthorization(filterContext);
            }
        }

        #region Private Helper Methods

        private bool IsSafeMethod(string httpMethod) {
            return httpMethod == "GET" || httpMethod == "HEAD";
        }

        private bool IsAntiForgeryProtectionEnabled(AuthorizationContext context) {

            var currentModule = GetArea(context.RouteData);
            return !String.IsNullOrEmpty(currentModule)
                   && (_extensionManager.AvailableExtensions()
                       .First(descriptor => String.Equals(descriptor.Id, currentModule, StringComparison.OrdinalIgnoreCase))
                       .AntiForgery.Equals("enabled", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetArea(RouteData routeData) {
            if (routeData.Values.ContainsKey("area"))
                return routeData.Values["area"] as string;

            return routeData.DataTokens["area"] as string ?? "";
        }

        private static bool ShouldValidate(AuthorizationContext context) {
            var attributes =
                (ValidateAntiForgeryTokenOrchardAttribute[])
                context.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenOrchardAttribute), false);

            if (attributes.Length > 0 && attributes[0].Enabled) {
                return true;
            }

            return false;
        }

        private static bool ShouldNotValidate(AuthorizationContext context) {
            var attributes =
                (ValidateAntiForgeryTokenOrchardAttribute[])
                context.ActionDescriptor.GetCustomAttributes(typeof(ValidateAntiForgeryTokenOrchardAttribute), false);

            if (attributes.Length > 0 && !attributes[0].Enabled) {
                return true;
            }

            return false;
        }

        #endregion
    }
}