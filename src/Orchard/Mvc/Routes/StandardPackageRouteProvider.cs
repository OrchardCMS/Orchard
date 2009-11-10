using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Packages;

namespace Orchard.Mvc.Routes {
    public class StandardPackageRouteProvider : IRouteProvider {
        private readonly IPackageManager _packageManager;

        public StandardPackageRouteProvider(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            foreach (var entry in _packageManager.ActivePackages()) {
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