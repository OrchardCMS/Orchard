using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.Mvc.AntiForgery {
    [UsedImplicitly]
    public class AntiForgeryAuthorizationFilter : FilterProvider, IAuthorizationFilter {
        private readonly ISiteService _siteService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IExtensionManager _extensionManager;

        public AntiForgeryAuthorizationFilter(ISiteService siteService, IAuthenticationService authenticationService, IExtensionManager extensionManager) {
            _siteService = siteService;
            _authenticationService = authenticationService;
            _extensionManager = extensionManager;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            if ((filterContext.HttpContext.Request.HttpMethod != "POST" ||
                 _authenticationService.GetAuthenticatedUser() == null) && !ShouldValidateGet(filterContext)) {
                return;
            }

            if (!IsAntiForgeryProtectionEnabled(filterContext)) {
                return;
            }

            var validator = new ValidateAntiForgeryTokenAttribute();
            validator.OnAuthorization(filterContext);

            if (filterContext.HttpContext is HackHttpContext)
                filterContext.HttpContext = ((HackHttpContext)filterContext.HttpContext).OriginalHttpContextBase;
        }

        private bool IsAntiForgeryProtectionEnabled(ControllerContext context) {
            string currentModule = GetArea(context.RouteData);
            if (!String.IsNullOrEmpty(currentModule)) {
                foreach (var descriptor in _extensionManager.AvailableExtensions()) {
                    if (String.Equals(descriptor.Id, currentModule, StringComparison.OrdinalIgnoreCase)) {
                        if (String.Equals(descriptor.AntiForgery, "enabled", StringComparison.OrdinalIgnoreCase)) {
                            return true;
                        }
                        return false;
                    }
                }
            }

            return false;
        }

        private static string GetArea(RouteData routeData) {
            if (routeData.Values.ContainsKey("area"))
                return routeData.Values["area"] as string;

            return routeData.DataTokens["area"] as string ?? "";
        }

        private static bool ShouldValidateGet(AuthorizationContext context) {
            const string tokenFieldName = "__RequestVerificationToken";

            var attributes =
                (ValidateAntiForgeryTokenOrchardAttribute[])
                context.ActionDescriptor.GetCustomAttributes(typeof (ValidateAntiForgeryTokenOrchardAttribute), false);

            if (attributes.Length > 0) {
                var request = context.HttpContext.Request;

                //HAACK: (erikpo) If the token is in the querystring, put it in the form so MVC can validate it
                if (!string.IsNullOrEmpty(request.QueryString[tokenFieldName])) {
                    context.HttpContext = new HackHttpContext(context.HttpContext, (HttpContext)context.HttpContext.Items["originalHttpContext"]);
                    ((HackHttpRequest)context.HttpContext.Request).AddFormValue(tokenFieldName, context.HttpContext.Request.QueryString[tokenFieldName]);
                }

                return true;
            }

            return false;
        }

        #region HackHttpContext

        private class HackHttpContext : HttpContextWrapper {
            private readonly HttpContextBase _originalHttpContextBase;
            private readonly HttpContext _originalHttpContext;
            private HttpRequestWrapper _request;

            public HackHttpContext(HttpContextBase httpContextBase, HttpContext httpContext)
                : base(httpContext) {
                _originalHttpContextBase = httpContextBase;
                _originalHttpContext = httpContext;
            }

            public HttpContextBase OriginalHttpContextBase {
                get { return _originalHttpContextBase; }
            }

            public override HttpRequestBase Request
            {
                get
                {
                    if (_request == null)
                        _request = new HackHttpRequest(_originalHttpContext.Request);

                    return _request;
                }
            }
        }

        #endregion

        #region HackHttpRequest

        private class HackHttpRequest : HttpRequestWrapper {
            private readonly HttpRequest _originalHttpRequest;
            private NameValueCollection _form;

            public HackHttpRequest(HttpRequest httpRequest)
                : base(httpRequest) {
                _originalHttpRequest = httpRequest;
            }

            public override NameValueCollection Form
            {
                get
                {
                    if (_form == null)
                        _form = new NameValueCollection(_originalHttpRequest.Form);

                    return _form;
                }
            }

            public void AddFormValue(string key, string value) {
                Form.Add(key, value);
            }
        }

        #endregion
    }
}