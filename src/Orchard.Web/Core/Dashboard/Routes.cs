using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Dashboard {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = -5,
                                                     Route = new Route(
                                                         "Admin",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Dashboard"},
                                                                                      {"controller", "admin"},
                                                                                      {"action", "index"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Dashboard"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}