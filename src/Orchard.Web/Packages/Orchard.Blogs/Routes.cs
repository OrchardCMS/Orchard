using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Blogs {
    //public class Routes : IRouteProvider {
    //    public void GetRoutes(ICollection<RouteDescriptor> routes) {
    //        foreach (var routeDescriptor in GetRoutes())
    //            routes.Add(routeDescriptor);
    //    }

    //    public IEnumerable<RouteDescriptor> GetRoutes() {
    //        return new[] {
    //                         new RouteDescriptor {
    //                                                 Route = new Route(
    //                                                     "{*slug}",
    //                                                     new RouteValueDictionary {
    //                                                                                  {"area", "Orchard.CmsPages"},
    //                                                                                  {"controller", "templates"},
    //                                                                                  {"action", "show"}
    //                                                                              },
    //                                                     new RouteValueDictionary {
    //                                                                                  {"slug", ""}
    //                                                                              },
    //                                                     new RouteValueDictionary {
    //                                                                                  {"area", "Orchard.CmsPages"}
    //                                                                              },
    //                                                     new MvcRouteHandler())
    //                                             }
    //                     };
    //    }
    //}
}