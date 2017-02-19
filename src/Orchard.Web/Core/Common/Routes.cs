using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Common {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptor = new RouteDescriptor {
                Priority = -9999,
                Route = new Route(
                        "{*path}",
                        new RouteValueDictionary {
                            {"area", "Common"},
                            {"controller", "Error"},
                            {"action", "NotFound"}
                        },
                        new RouteValueDictionary {
                        },
                        new RouteValueDictionary {
                            {"area", "Common"}
                        },
                        new MvcRouteHandler())
            };

            routes.Add(routeDescriptor);
        }
    }
}