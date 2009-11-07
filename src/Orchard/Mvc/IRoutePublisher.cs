using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Mvc {
    public interface IRoutePublisher : IDependency {
        void Publish(IEnumerable<RouteDescriptor> routes);
    }
}