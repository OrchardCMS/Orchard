using System.Collections.Generic;
using Orchard.Mvc.Routes;

namespace Orchard.WebApi.Routes {
    public interface IHttpRouteProvider : IDependency {
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}
