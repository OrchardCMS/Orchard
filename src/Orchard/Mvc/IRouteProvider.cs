using System.Collections.Generic;

namespace Orchard.Mvc {
    public interface IRouteProvider : IDependency {
        IEnumerable<RouteDescriptor> GetRoutes();
    }
}
