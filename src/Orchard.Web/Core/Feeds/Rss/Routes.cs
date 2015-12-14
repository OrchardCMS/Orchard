using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Feeds.Rss {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = -5,
                                                     Route = new Route(
                                                         "rss",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Feeds"},
                                                                                      {"controller", "Feed"},
                                                                                      {"action", "Index"},
                                                                                      {"format", "rss"},
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Feeds"}
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