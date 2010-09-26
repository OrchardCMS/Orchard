using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Routable {
    public class Routes : IRouteProvider {
         private readonly IRoutablePathConstraint _routablePathConstraint;

        public Routes(IRoutablePathConstraint routablePathConstraint) {
            _routablePathConstraint = routablePathConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Common/Routable/Slugify",
                        new RouteValueDictionary {
                            {"area", "Routable"},
                            {"controller", "Item"},
                            {"action", "Slugify"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Routable"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 10,
                    Route = new Route(
                        "{*path}",
                        new RouteValueDictionary {
                            {"area", "Routable"},
                            {"controller", "Item"},
                            {"action", "Display"}
                        },
                        new RouteValueDictionary {
                            {"path", _routablePathConstraint}
                        },
                        new RouteValueDictionary {
                            {"area", "Routable"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

   }
}