using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.MultiTenancy {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/MultiTenancy/Edit/{name}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.MultiTenancy"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"name", ".+"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.MultiTenancy"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }
    }
}