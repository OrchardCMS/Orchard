using System.Web;
using Autofac;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        private readonly IComponentContext _context;

        public HttpContextAccessor(IComponentContext context) {
            _context = context;
        }

        public HttpContextBase Current() {
            // TODO: HttpContextBase is not registred in the "shell" lifetime scope, so resolving it will cause an exception.
            
            return HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : null;
        }
    }
}