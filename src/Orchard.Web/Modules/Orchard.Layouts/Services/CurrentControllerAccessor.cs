using System.Web;
using System.Web.Mvc;
using Orchard.Layouts.Filters;

namespace Orchard.Layouts.Services {
    public class CurrentControllerAccessor : ICurrentControllerAccessor {
        private readonly IWorkContextAccessor _wca;
        private readonly HttpContextBase _httpContext;

        public CurrentControllerAccessor(IWorkContextAccessor wca, HttpContextBase httpContext) {
            _wca = wca;
            _httpContext = httpContext;
        }

        public Controller CurrentController {
            get { return (Controller) _httpContext.Items[ControllerAccessorFilter.CurrentControllerKey]; }
        }
    }
}