using System.Collections.Generic;

namespace Orchard.Mvc.Routes {
    public interface IRouteProvider : IDependency {
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}