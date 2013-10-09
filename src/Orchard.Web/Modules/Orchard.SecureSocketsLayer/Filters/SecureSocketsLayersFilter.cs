using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.SecureSocketsLayer.Services;

namespace Orchard.SecureSocketsLayer.Filters {
    public class SecureSocketsLayersFilter : FilterProvider, IActionFilter {
        private readonly ISecureSocketsLayerService _sslService;

        public SecureSocketsLayersFilter(ISecureSocketsLayerService sslService) {
            _sslService = sslService;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {}

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var user = filterContext.HttpContext.User;
            var secure =
                (user != null && user.Identity.IsAuthenticated) ||
                _sslService.ShouldBeSecure(filterContext);

            var request = filterContext.HttpContext.Request;

            // redirect to a secured connection ?
            if (secure && !request.IsSecureConnection) {
                var secureActionUrl = AppendQueryString(
                    request.QueryString,
                    _sslService.SecureActionUrl(
                        filterContext.ActionDescriptor.ActionName,
                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                        filterContext.RequestContext.RouteData.Values));

                filterContext.Result = new RedirectResult(secureActionUrl);
                return;
            }

            // non auth page on a secure canal
            // nb: needed as the ReturnUrl for LogOn doesn't force the scheme to http, and reuses the current one
            if (!secure && request.IsSecureConnection) {
                var insecureActionUrl = AppendQueryString(
                    request.QueryString,
                    _sslService.InsecureActionUrl(
                        filterContext.ActionDescriptor.ActionName,
                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                        filterContext.RequestContext.RouteData.Values));

                filterContext.Result = new RedirectResult(insecureActionUrl);
            }
        }

        private static string AppendQueryString(NameValueCollection queryString, string url) {
            if (queryString.Count > 0) {
                url += '?' + queryString.ToString();
            }
            return url;
        }
    }
}