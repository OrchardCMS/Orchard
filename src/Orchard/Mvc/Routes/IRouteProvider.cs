using System.Collections.Generic;

namespace Orchard.Mvc.Routes {
    public interface IRouteProvider : IDependency {
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}