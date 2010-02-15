using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.HomePage {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = 20,
                                                     Route = new Route(
                                                         "",
                                                         new RouteValueDictionary {
                                                                                      {"area", "HomePage"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "Index"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "HomePage"},
                                                                                      {"controller", "Home"},
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "HomePage"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}
