using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Migrations {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Migrations/",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Migrations"},
                                                                                      {"controller", "DatabaseUpdate"},
                                                                                      {"action", "Index"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Migrations"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Migrations/UpdateDatabase",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Migrations"},
                                                                                      {"controller", "DatabaseUpdate"},
                                                                                      {"action", "UpdateDatabase"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Migrations"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}