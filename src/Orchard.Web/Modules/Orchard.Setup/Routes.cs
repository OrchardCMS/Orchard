using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Setup {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "{controller}/{action}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Setup"},
                            {"controller", "Setup"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Setup"},
                            {"controller", "Setup"},
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Setup"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}