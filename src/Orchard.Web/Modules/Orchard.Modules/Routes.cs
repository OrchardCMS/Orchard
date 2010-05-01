using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Modules.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Modules {
    public class Routes : IRouteProvider {
        private readonly IModuleNameConstraint _moduleNameConstraint;
        private readonly IFeatureNameConstraint _featureNameConstraint;

        public Routes(IModuleNameConstraint moduleNameConstraint, IFeatureNameConstraint featureNameConstraint) {
            _moduleNameConstraint = moduleNameConstraint;
            _featureNameConstraint = featureNameConstraint;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Modules/Edit/{moduleName}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"moduleName", _moduleNameConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Modules/Enable/{featureName}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Enable"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"featureName", _featureNameConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Modules/Disable/{featureName}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Disable"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"featureName", _featureNameConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Modules"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}