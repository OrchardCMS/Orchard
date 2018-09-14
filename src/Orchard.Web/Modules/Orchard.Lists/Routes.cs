using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Lists {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptors = new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/Lists/TreeViewSource",
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"},
                            {"controller", "AdminTreeView"},
                            {"action", "Nodes"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/Lists/{containerId}/{filterByContentType}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"},
                            {"controller", "Admin"},
                            {"action", "List"},
                            {"filterByContentType", ""}
                        },
                        new RouteValueDictionary{
                            {"filterByContentType", @"\w*"},
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
                        "Admin/Lists/Choose/From/Orphaned/To/{targetContainerId}/{filterByContentType}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"},
                            {"controller", "Admin"},
                            {"action", "Choose"},
                            {"filterByContentType", ""},
                            {"sourceContainerId", "0"}
                        },
                        new RouteValueDictionary{
                            {"filterByContentType", @"\w*"},
                            {"targetContainerId", @"\d+"},
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/Lists/Choose/From/{sourceContainerId}/To/{targetContainerId}/{filterByContentType}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"},
                            {"controller", "Admin"},
                            {"action", "Choose"},
                            {"filterByContentType", ""}
                        },
                        new RouteValueDictionary{
                            {"filterByContentType", @"\w*"},
                            {"sourceContainerId", @"\d+"},
                            {"targetContainerId", @"\d+"},
                        },
                        new RouteValueDictionary {
                            {"area", "Orchard.Lists"}
                        },
                        new MvcRouteHandler())
                },
            };

            foreach (var routeDescriptor in routeDescriptors) {
                routes.Add(routeDescriptor);
            }
        }
    }
}