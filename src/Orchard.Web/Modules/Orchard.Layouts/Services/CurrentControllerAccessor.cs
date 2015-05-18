using System.Web.Mvc;
using Orchard.Layouts.Filters;
using Orchard.Mvc;

namespace Orchard.Layouts.Services {
    public class CurrentControllerAccessor : ICurrentControllerAccessor {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentControllerAccessor(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public Controller CurrentController {
            get { return (Controller) _httpContextAccessor.Current().Items[ControllerAccessorFilter.CurrentControllerKey]; }
        }
    }
}