using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace PI.Authentication {
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
                                                         "Auth/LogOn",
                                                         new RouteValueDictionary {
                                                                                      {"area", "PI.Authentication"},
                                                                                      {"controller", "Account"},
                                                                                      {"action", "index"}
                                                                                  },
                                                         null,
                                                         new RouteValueDictionary {
                                                                                      {"area", "PI.Authentication"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
            };
        }
    }
}