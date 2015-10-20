using System;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Orchard.Mvc {

    public class HttpContextAccessor : IHttpContextAccessor {
        private HttpContextBase _httpContext;

        public HttpContextBase Current() {
            var httpContext = GetStaticProperty();
            return !IsBackgroundHttpContext(httpContext) ? new HttpContextWrapper(httpContext) :
                _httpContext ?? CallContext.LogicalGetData("HttpContext") as HttpContextBase;
        }

        public void Set(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        private static bool IsBackgroundHttpContext(HttpContext httpContext) {
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
