using System.Web;
using Autofac;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        private readonly IComponentContext _context;

        public HttpContextAccessor(IComponentContext context) {
            _context = context;
        }

        public HttpContextBase Current() {
            HttpContextBase httpContextBase;
            _context.TryResolve(out httpContextBase);
            return httpContextBase;
        }
    }
}