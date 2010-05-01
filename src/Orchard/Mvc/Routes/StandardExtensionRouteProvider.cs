using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Environment.Topology.Models;

namespace Orchard.Mvc.Routes {
    public class StandardExtensionRouteProvider : IRouteProvider {
        private readonly ShellTopology _topology;

        public StandardExtensionRouteProvider(ShellTopology topology) {
            _topology = topology;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            var displayNamesPerArea = _topology.Controllers.GroupBy(
                x => x.AreaName,
                x => x.Feature.Descriptor.Extension.DisplayName);

            foreach (var item in displayNamesPerArea) {
                var areaName = item.Key;
                var displayName = item.Distinct().Single();

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