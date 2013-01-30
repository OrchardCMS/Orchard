using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;
using Contrib.Taxonomies.Routing;

namespace Contrib.Taxonomies {
    public class Routes : IRouteProvider {
        private readonly ITaxonomySlugConstraint _taxonomySlugConstraint;
        private readonly ITermPathConstraint _termPathConstraint;

        public Routes(ITaxonomySlugConstraint taxonomySlugConstraint, ITermPathConstraint termPathConstraint) {
            _taxonomySlugConstraint = taxonomySlugConstraint;
            _termPathConstraint = termPathConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                    Priority = 90,
                                                    Route = new Route(
                                                         "{taxonomySlug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Taxonomies"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "List"},
                                                                                      {"taxonomySlug", ""}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"taxonomySlug", _taxonomySlugConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Taxonomies"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                    Priority = 90,
                                                    Route = new Route(
                                                         "{*termPath}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Taxonomies"},
                                                                                      {"controller", "Home"},
                                                                                      {"action", "Item"},
                                                                                      {"termPath", ""}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"termPath", _termPathConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Contrib.Taxonomies"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}