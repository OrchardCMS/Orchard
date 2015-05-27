using System;
using System.Web;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        private HttpContextBase _httpContext;

        public HttpContextBase Current() {
            var httpContext = GetStaticProperty();
            return !IsBackgroundHttpContext(httpContext) ? new HttpContextWrapper(httpContext) : _httpContext;
        }

        public void Set(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        private static bool IsBackgroundHttpContext(HttpContext httpContext) {
            return httpContext == null || httpContext.Items.Contains(BackgroundHttpContextFactory.IsBackgroundHttpContextKey);
        }

        private static HttpContext GetStaticProperty() {
            var httpContext = HttpContext.Current;
            if (httpContext == null) {
                return null;
        }

        public HttpContextBase Current() {
            // TODO: HttpContextBase is not registred in the "shell" lifetime scope, so resolving it will cause an exception.
            
            return HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : null;
        }
    }
}