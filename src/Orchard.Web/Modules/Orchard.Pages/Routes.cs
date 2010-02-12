using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Orchard.Pages.Services;

namespace Orchard.Pages {
    public class Routes : IRouteProvider {
        private readonly ISlugConstraint _slugConstraint;

        public Routes(ISlugConstraint slugConstraint) {
            _slugConstraint = slugConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = 10,
                                                     Route = new Route(
                                                         "{*slug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Pages"},
                                                                                      {"controller", "page"},
                                                                                      {"action", "item"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"slug", _slugConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Pages"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

    }
}
