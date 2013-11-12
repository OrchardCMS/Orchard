using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Filters;
using Orchard.SecureSocketsLayer.Services;
using Orchard.UI.Notify;

namespace Orchard.SecureSocketsLayer.Filters {
    public class SecureSocketsLayersFilter : FilterProvider, IActionFilter {
        private readonly ISecureSocketsLayerService _sslService;
        private readonly IOrchardServices _orchardServices;

        public SecureSocketsLayersFilter(ISecureSocketsLayerService sslService, IOrchardServices orchardServices) {
            _sslService = sslService;
            _orchardServices = orchardServices;
        }
        public Localizer T { get; set; }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var settings = _sslService.GetSettings();

            if (!settings.Enabled) {
                _orchardServices.Notifier.Warning(T("You need to configure the SSL settings."));
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var settings = _sslService.GetSettings();

            if (!settings.Enabled) {
                return;
            }

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