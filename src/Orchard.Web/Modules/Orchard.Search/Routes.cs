﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Search {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptors = new[] {
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
                },
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route("Admin/Search/BlogSearch/{blogId}",
                        new RouteValueDictionary {
                            {"area", "Orchard.Search"},
                            {"controller", "BlogSearch"},
                            {"action", "Index"},
                            {"blogId", UrlParameter.Optional}
                        },
                        null,
                        new RouteValueDictionary {
                            {"area", "Orchard.Search"}
                        },
                        new MvcRouteHandler())
                }
            };

            foreach (var routeDescriptor in routeDescriptors)
                routes.Add(routeDescriptor);
        }
    }
}
