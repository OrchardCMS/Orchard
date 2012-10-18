using System.Collections.Generic;
using Orchard.Mvc.Routes;

namespace Orchard.WebApi.Routes {
    public interface IHttpRouteProvider : IDependency {
        IEnumerable<RouteDescriptor> GetRoutes();
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}
