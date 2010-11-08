using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Containers {
    public class Routes : IRouteProvider {
         private readonly IContainersPathConstraint _containersPathConstraint;

         public Routes(IContainersPathConstraint containersPathConstraint) {
             _containersPathConstraint = containersPathConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 15,
                    Route = new Route(
                        "{*path}",
                        new RouteValueDictionary {
                            {"area", "Containers"},
                            {"controller", "Item"},
                            {"action", "Display"}
                        },
                        new RouteValueDictionary {
                            {"path", _containersPathConstraint}
                        },
                        new RouteValueDictionary {
                            {"area", "Containers"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

   }
}