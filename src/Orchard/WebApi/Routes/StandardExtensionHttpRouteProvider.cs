using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Mvc.Routes;

namespace Orchard.WebApi.Routes {
    public class StandardExtensionHttpRouteProvider : IHttpRouteProvider {
        private readonly ShellBlueprint _blueprint;

        public StandardExtensionHttpRouteProvider(ShellBlueprint blueprint) {
            _blueprint = blueprint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var displayPathsPerArea = _blueprint.HttpControllers.GroupBy(
                x => x.AreaName,
                x => x.Feature.Descriptor.Extension.Path);

            foreach (var item in displayPathsPerArea) {
                var areaName = item.Key;
                var displayPath = item.Distinct().Single();

                var routeDescriptor = new HttpRouteDescriptor {
                    Priority = -10,
                    RouteTemplate = "api/" + displayPath + "/{controller}/{id}",
                    Defaults = new { area = areaName, controller = "api", id = RouteParameter.Optional }
                };

                routes.Add(routeDescriptor);
            }
        }
    }
}