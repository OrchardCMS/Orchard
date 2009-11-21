using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Blogs {
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Blogs",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             //new RouteDescriptor {
                             //                        Route = new Route(
                             //                            "{blogSlug}",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Orchard.Blogs"},
                             //                                                         {"controller", "Blog"},
                             //                                                         {"action", "Item"}
                             //                                                     },
                             //                            new RouteValueDictionary()/* {
                             //                                                         {"blogSlug", new IsBlogConstraint()}
                             //                                                     }*/,
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Orchard.Blogs"}
                             //                                                     },
                             //                            new MvcRouteHandler())
                             //                    }
                         };
        }
    }
}