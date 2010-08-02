using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Contents {
    public class Routes : IRouteProvider {
        #region IRouteProvider Members

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
                        "Admin/Contents/List/{id}/InContainer/{containerId}",
                        new RouteValueDictionary {
                            {"area", "Contents"},
                            {"controller", "Admin"},
                            {"action", "List"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Contents"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        #endregion
    }
}