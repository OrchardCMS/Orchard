using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Recipes {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {   Priority = 5,
                                                     Route = new Route(
                                                         "Recipes/Status/{executionId}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Recipes"},
                                                                                      {"controller", "Recipes"},
                                                                                      {"action", "RecipeExecutionStatus"}
                                                         },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Recipes"}
                                                         },
                                                         new MvcRouteHandler())
                             }
                         };
        }
    }
}