using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Extensions;

namespace Orchard.Mvc.Routes {
    public class StandardExtensionRouteProvider : IRouteProvider {
        private readonly IExtensionManager _extensionManager;

        public StandardExtensionRouteProvider(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            foreach (var entry in _extensionManager.ActiveExtensions()) {
                var areaName = entry.Descriptor.Name;
                var displayName = entry.Descriptor.DisplayName ?? areaName;

                yield return new RouteDescriptor {
                    Priority = -10,
                    Route = new Route(
                        "Admin/" + displayName + "/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "admin"},
                            {"action", "index"},
                            {"id", ""}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", areaName}
                        },
                        new MvcRouteHandler())
                };
                yield return new RouteDescriptor {
                    Priority = -10,
                    Route = new Route(
                        displayName + "/{controller}/{action}/{id}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "home"},
                            {"action", "index"},
                            {"id", ""}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", areaName}
                        },
                        new MvcRouteHandler())
                };
            }
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

    }
}