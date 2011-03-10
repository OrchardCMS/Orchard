using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Lists {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/Orchard.Lists/{containerId}/{filterByContentType}",
                        new RouteValueDictionary {
                            {"area", "Contents"},
                            {"controller", "Admin"},
                            {"action", "List"},
                            {"filterByContentType", ""}
                        },
                        new RouteValueDictionary{
                            {"filterByContentType", @"\w+"},
                            {"containerId", @"\d+"}
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/Orchard.Lists/{containerId}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"},
                            {"controller", "Admin"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary {
                            {"containerId", @"\d+"}
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}