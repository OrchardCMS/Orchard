using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Modules.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Modules {
    public class Routes : IRouteProvider {
        private readonly IModuleNameConstraint _moduleNameConstraint;

        public Routes(IModuleNameConstraint moduleNameConstraint) {
            _moduleNameConstraint = moduleNameConstraint;
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
                                                 }
                         };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}