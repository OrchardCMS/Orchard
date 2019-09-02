using System.Web;
using Orchard.Mvc;

namespace Orchard.Tests.Stubs {
    public class StubHttpContextAccessor : IHttpContextAccessor {
        private HttpContextBase _httpContext;

        public StubHttpContextAccessor() {
        }

        public StubHttpContextAccessor(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        public HttpContextBase Current() {
            return _httpContext;
        }

        public void Set(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }
    }
}