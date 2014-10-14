using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Orchard.ImportExport.Routes {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentApiRoutes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route("api/deployment/{controller}/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", "Orchard.ImportExport"},
                            {"id", null}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.ImportExport"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}