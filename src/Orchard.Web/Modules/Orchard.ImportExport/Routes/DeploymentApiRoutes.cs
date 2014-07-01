using System.Collections.Generic;
using System.Web.Http;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;

namespace Orchard.ImportExport.Routes {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentApiRoutes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/deployment/{controller}/{action}/{id}",
                    Defaults = new {
                        area = "Orchard.ImportExport",
                        id = RouteParameter.Optional
                    }
                }
            };
        }
    }
}