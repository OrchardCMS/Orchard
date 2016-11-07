using System;
using System.Web;
using Autofac;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        readonly ILifetimeScope _lifetimeScope;
        private HttpContextBase _httpContext;
        private IWorkContextAccessor _wca;

        public HttpContextAccessor(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public HttpContextBase Current() {
            var httpContext = GetStaticProperty();

            if (!IsBackgroundHttpContext(httpContext))
                return new HttpContextWrapper(httpContext);

            if (_httpContext != null)
                return _httpContext;

            if (_wca == null && _lifetimeScope.IsRegistered<IWorkContextAccessor>())
                _wca = _lifetimeScope.Resolve<IWorkContextAccessor>();

            var workContext = _wca != null ? _wca.GetLogicalContext() : null;
            return workContext != null ? workContext.HttpContext : null;
        }

        public void Set(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        internal static bool IsBackgroundHttpContext(HttpContext httpContext) {
            return httpContext == null || httpContext.Items.Contains(MvcModule.IsBackgroundHttpContextKey);
        }

        private static HttpContext GetStaticProperty() {
            var httpContext = HttpContext.Current;
            if (httpContext == null) {
                return null;
            }

            try {
                // The "Request" property throws at application startup on IIS integrated pipeline mode.
                if (httpContext.Request == null) {
                    return null;
                }
            }
            catch (Exception) {
                return null;
            }
            return httpContext;
        }
    }
}
