using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Orchard.Pages.Routing;

namespace Orchard.Pages {
    public class Routes : IRouteProvider {
        private readonly IPageSlugConstraint _pageSlugConstraint;

        public Routes(IPageSlugConstraint pageSlugConstraint) {
            _pageSlugConstraint = pageSlugConstraint;
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
                                                                                      {"slug", _pageSlugConstraint}
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
