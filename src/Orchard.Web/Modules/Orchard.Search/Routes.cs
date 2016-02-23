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
                    Route = new Route("Search/ContentPicker",
                        new RouteValueDictionary {
                            {"area", "Orchard.Search"},
                            {"controller", "ContentPicker"},
                            {"action", "Index"}
                        },
                        null,
                        new RouteValueDictionary {
                            {"area", "Orchard.Search"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route("Search/{searchIndex}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Search"},
                            {"controller", "Search"},
                            {"action", "Index"},
                            {"searchIndex", UrlParameter.Optional}
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
