using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.CmsPages.Services;
using Orchard.Mvc.Routes;

namespace Orchard.CmsPages {

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
                                                                                      {"area", "Orchard.CmsPages"},
                                                                                      {"controller", "templates"},
                                                                                      {"action", "show"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"slug", _slugConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

    }
}
