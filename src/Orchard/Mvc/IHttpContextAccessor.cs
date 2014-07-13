using System;
using System.Web;

namespace Orchard.Mvc {
    public interface IHttpContextAccessor {
        HttpContextBase Current();
        void Set(HttpContextBase stub);
    }

    public class HttpContextAccessor : IHttpContextAccessor {
        private HttpContextBase _stub;

        public HttpContextBase Current() {
            var httpContext = GetStaticProperty();
            return httpContext != null ? new HttpContextWrapper(httpContext) : _stub;
        }

        public void Set(HttpContextBase stub) {
            _stub = stub;
        }

        private HttpContext GetStaticProperty() {
            var httpContext = HttpContext.Current;
            if (httpContext == null) {
                return null;
            }

            try {
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
