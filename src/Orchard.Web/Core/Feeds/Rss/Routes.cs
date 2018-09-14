using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Feeds.Rss {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptor = new RouteDescriptor {
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
            };

            routes.Add(routeDescriptor);
        }
    }
}