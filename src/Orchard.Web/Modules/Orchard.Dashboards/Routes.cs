using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Dashboards {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            yield return new RouteDescriptor {
                Priority = -4,
                Route = new Route(
                    "Admin",
                    new RouteValueDictionary {
                        {"area", "Orchard.Dashboards"},
                        {"controller", "Admin"},
                        {"action", "Index"}
                    },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                        {"area", "Orchard.Dashboards"}
                    },
                    new MvcRouteHandler())
            };
        }
    }
}