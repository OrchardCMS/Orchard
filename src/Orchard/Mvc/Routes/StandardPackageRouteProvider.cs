using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Mvc.Routes {
    public class StandardPackageRouteProvider : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            //TEMP: Need to rely on a service that gives the list of active packages
            return new[] {
                             new RouteDescriptor {
                                                     Priority = -10,
                                                     Route = new Route(
                                                         "Admin/Pages/{action}/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"},
                                                                                      {"controller", "admin"},
                                                                                      {"action", "index"},
                                                                                      {"id", ""}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = -10,
                                                     Route = new Route(
                                                         "Pages/{controller}/{action}/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"},
                                                                                      {"controller", "home"},
                                                                                      {"action", "index"},
                                                                                      {"id", ""}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = -10,
                                                     Route = new Route(
                                                         "Admin/Media/{action}/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Media"},
                                                                                      {"controller", "admin"},
                                                                                      {"action", "index"},
                                                                                      {"id", ""}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Media"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = -10,
                                                     Route = new Route(
                                                         "XmlRpc/{controller}/{action}/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.XmlRpc"},
                                                                                      {"controller", "home"},
                                                                                      {"action", "index"},
                                                                                      {"id", ""}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.XmlRpc"},
                                                                                      {"namespace", "Orchard.XmlRpc.Controllers"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                         };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

    }
}