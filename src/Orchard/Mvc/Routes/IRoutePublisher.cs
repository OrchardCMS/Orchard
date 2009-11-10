using System.Collections.Generic;

namespace Orchard.Mvc.Routes {
    public interface IRoutePublisher : IDependency {
        void Publish(IEnumerable<RouteDescriptor> routes);
    }
}