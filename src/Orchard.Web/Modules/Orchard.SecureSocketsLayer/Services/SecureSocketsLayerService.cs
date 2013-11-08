using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.SecureSocketsLayer.Models;
using Orchard.UI.Admin;

namespace Orchard.SecureSocketsLayer.Services {
    public class SecureSocketsLayerService : ISecureSocketsLayerService {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public SecureSocketsLayerService(
            IWorkContextAccessor workContextAccessor, 
            ICacheManager cacheManager,
            ISignals signals) {
            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public bool ShouldBeSecure(string actionName, string controllerName, RouteValueDictionary routeValues) {
            var requestContext = GetRequestContext(actionName, controllerName, routeValues);
            return ShouldBeSecure(requestContext, null);
        }

        public bool ShouldBeSecure(RequestContext requestContext) {
            return ShouldBeSecure(requestContext, null);
        }

        public bool ShouldBeSecure(ActionExecutingContext actionContext) {
            var requestContext = GetRequestContext(
                actionContext.ActionDescriptor.ActionName,
                actionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                actionContext.RequestContext.RouteData.Values);
            return ShouldBeSecure(requestContext, actionContext);
        }

        private bool ShouldBeSecure(RequestContext requestContext, ActionExecutingContext actionContext) {
            var controllerName = (string) requestContext.RouteData.Values["controller"];
            if (controllerName == null) return false;
            var actionName = (string) requestContext.RouteData.Values["action"];
            if (actionName == null) return false;

            var settings = GetSettings();
            if (settings == null || !settings.Enabled) {
                return false;
            }

            if (actionName.EndsWith("Ssl") || controllerName.EndsWith("Ssl")) {
                return true;
            }

            var controller = (actionContext != null
                ? actionContext.Controller
                : ControllerBuilder.Current.GetControllerFactory()
                    .CreateController(requestContext, controllerName)) as ControllerBase;
            if (controller != null) {
                var controllerType = controller.GetType();
                if (controllerType.GetCustomAttributes(typeof(RequireHttpsAttribute), false).Any()) {
                    return true;
                }
                ActionDescriptor actionDescriptor;
                if (actionContext != null) {
                    actionDescriptor = actionContext.ActionDescriptor;
                }
                else {
                    var controllerContext = new ControllerContext(requestContext, controller);
                    var controllerDescriptor = new ReflectedControllerDescriptor(controllerType);
                    actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);
                }
                if (actionDescriptor.GetCustomAttributes(typeof(RequireHttpsAttribute), false).Any()) {
                    return true;
                }
            }

            if (settings.SecureEverything) return true;

            if (controllerName == "Account" &&
                (actionName == "LogOn"
                 || actionName == "ChangePassword"
                 || actionName == "AccessDenied"
                 || actionName == "Register"
                 || actionName.StartsWith("ChallengeEmail", StringComparison.OrdinalIgnoreCase))) {
                return true;
            }

            if (controllerName == "Admin" || AdminFilter.IsApplied(requestContext)) {
                return true;
            }

            if (!settings.CustomEnabled) return false;

            var urlHelper = new UrlHelper(requestContext);
            var url = urlHelper.Action(actionName, controllerName, requestContext.RouteData);

            return IsRequestProtected(
                url, requestContext.HttpContext.Request.ApplicationPath, settings);
        }

        public string SecureActionUrl(string actionName, string controllerName) {
            return SecureActionUrl(actionName, controllerName, new object());
        }

        public string SecureActionUrl(string actionName, string controllerName, object routeValues) {
            return SecureActionUrl(actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public string SecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues) {
            var requestContext = GetRequestContext(actionName, controllerName, routeValues);
            var url = new UrlHelper(requestContext);
            var actionUrl = url.Action(actionName, controllerName, routeValues);
            if (actionUrl == null) return null;
            var currentUri = _workContextAccessor.GetContext().HttpContext.Request.Url;
            return currentUri != null && currentUri.Scheme.Equals(Uri.UriSchemeHttps) ?
                actionUrl /* action url is relative so will keep current protocol */ :
                MakeSecure(actionUrl);
        }

        public string InsecureActionUrl(string actionName, string controllerName) {
            return InsecureActionUrl(actionName, controllerName, new object());
        }

        public string InsecureActionUrl(string actionName, string controllerName, object routeValues) {
            return InsecureActionUrl(actionName, controllerName, new RouteValueDictionary(routeValues));
        }

        public string InsecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues) {
            var requestContext = GetRequestContext(actionName, controllerName, routeValues);
            var url = new UrlHelper(requestContext);
            var actionUrl = url.Action(actionName, controllerName, routeValues);
            if (actionUrl == null) return null;
            var currentUri = _workContextAccessor.GetContext().HttpContext.Request.Url;
            return currentUri != null && currentUri.Scheme.Equals(Uri.UriSchemeHttp) ?
                actionUrl /* action url is relative so will keep current protocol */ :
                MakeInsecure(actionUrl);
        }

        private RequestContext GetRequestContext(
            string actionName,
            string controllerName,
            RouteValueDictionary routeValues) {
            
            var httpContext = _workContextAccessor.GetContext().HttpContext;
            var routeData = new RouteData();
            foreach (var routeValue in routeValues) {
                routeData.Values[routeValue.Key] = routeValue.Value;
            }
            routeData.Values["controller"] = controllerName;
            routeData.Values["action"] = actionName;
            var requestContext = new RequestContext(httpContext, routeData);
            return requestContext;
        }

        private static bool IsRequestProtected(string path, string appPath, SslSettings settings) {
            var match = false;
            var sr = new StringReader(settings.Urls ?? "");
            string pattern;

            while (!match && null != (pattern = sr.ReadLine())) {
                pattern = pattern.Trim();
                match = IsMatched(pattern, path, appPath);
            }

            return match;
        }

        private static bool IsMatched(string pattern, string path, string appPath) {
            if (pattern.StartsWith("~/")) {
                pattern = pattern.Substring(2);
                if (appPath == "/")
                    appPath = "";
                pattern = string.Format("{0}/{1}", appPath, pattern);
            }

            if (!pattern.Contains("?"))
                pattern = pattern.TrimEnd('/');

            var requestPath = path;
            if (!requestPath.Contains("?"))
                requestPath = requestPath.TrimEnd('/');

            return pattern.EndsWith("*")
                ? requestPath.StartsWith(pattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                : string.Equals(requestPath, pattern, StringComparison.OrdinalIgnoreCase);
        }

        public SslSettings GetSettings() {
            return _cacheManager.Get("SslSettings",
                ctx => {
                    ctx.Monitor(_signals.When(SslSettingsPart.CacheKey));
                    var settingsPart = _workContextAccessor.GetContext().CurrentSite.As<SslSettingsPart>();
                    return new SslSettings {
                        Urls = settingsPart.Urls,
                        CustomEnabled = settingsPart.CustomEnabled,
                        SecureEverything = settingsPart.SecureEverything,
                        SecureHostName = settingsPart.SecureHostName,
                        InsecureHostName = settingsPart.InsecureHostName,
                        Enabled = settingsPart.Enabled
                    };
                });
        }

        private string MakeInsecure(string path) {
            var settings = GetSettings();
            if (settings == null) return path;
            var builder = new UriBuilder {
                Scheme = Uri.UriSchemeHttp,
                Port = 80
            };
            var insecureHostName = settings.InsecureHostName;
            SetHost(insecureHostName, builder);
            builder.Path = path;
            return builder.Uri.ToString();
        }

        private string MakeSecure(string path) {
            var settings = GetSettings();
            if (settings == null) return path;
            var builder = new UriBuilder {
                Scheme = Uri.UriSchemeHttps,
                Port = 443
            };
            var secureHostName = settings.SecureHostName;
            SetHost(secureHostName, builder);
            builder.Path = path;
            return builder.Uri.ToString();
        }

        private static void SetHost(string hostName, UriBuilder builder) {
            if (string.IsNullOrWhiteSpace(hostName)) return;
            var splitSecuredHostName = hostName.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            if (splitSecuredHostName.Length == 2) {
                int port;
                if (int.TryParse(splitSecuredHostName[1], NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out port)) {
                    builder.Port = port;
                    hostName = splitSecuredHostName[0];
                }
            }
            builder.Host = hostName;
        }
    }
}