using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Search {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = 5,
                                                     Route = new Route(
                                                         "Search",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Search"},
                                                                                      {"controller", "search"},
                                                                                      {"action", "index"}
                                                                                  },
                                                         null,
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Search"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}