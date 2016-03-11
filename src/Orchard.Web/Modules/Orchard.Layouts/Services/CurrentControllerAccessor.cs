using System.Web;
using System.Web.Mvc;
using Orchard.Layouts.Filters;

namespace Orchard.Layouts.Services {
    public class CurrentControllerAccessor : ICurrentControllerAccessor {
        private readonly HttpContextBase _httpContext;

        public CurrentControllerAccessor(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        public Controller CurrentController {
            get { return (Controller) _httpContext.Items[ControllerAccessorFilter.CurrentControllerKey]; }
        }
    }
}