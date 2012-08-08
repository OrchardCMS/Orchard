using System.Collections.Generic;
using System.Linq;
using System.Web.Http.WebHost;
using System.Web.Http.WebHost.Routing;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Mvc.Routes;

namespace Orchard.WebApi.Routes {
    public class StandardExtensionHttpRouteProvider : IHttpRouteProvider {
        private readonly ShellBlueprint _blueprint;

        public StandardExtensionHttpRouteProvider(ShellBlueprint blueprint) {
            _blueprint = blueprint;
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            var displayPathsPerArea = _blueprint.HttpControllers.GroupBy(
                x => x.AreaName,
                x => x.Feature.Descriptor.Extension.Path);

            foreach (var item in displayPathsPerArea) {
                var areaName = item.Key;
                var displayPath = item.Distinct().Single();

                yield return new RouteDescriptor {
                    Priority = -10,
                    Route = new HttpWebRoute(
                        "api/" + displayPath + "/{controller}/{id}",
                        new RouteValueDictionary {
                            {"area", areaName},
                            {"controller", "api"},
                            {"id", ""}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", areaName}
                        },
                        HttpControllerRouteHandler.Instance)
                };
            }
        }


        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}