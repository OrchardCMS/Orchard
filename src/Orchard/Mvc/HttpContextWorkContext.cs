using System;

namespace Orchard.Mvc {
    public class HttpContextWorkContext : IWorkContextStateProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextWorkContext(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public Func<WorkContext, T> Get<T>(string name) {
            if (name == "HttpContext") {
                var result = (T)(object)_httpContextAccessor.Current();
                return ctx => result;
            }
            return null;
        }
    }
}
