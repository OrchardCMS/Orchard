using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Alias.Implementation;
using Orchard.Alias.Implementation.Holder;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Mvc.Routes;

namespace Orchard.Alias {
    public class Routes : IRouteProvider {
        private readonly ShellBlueprint _blueprint;
        private readonly IAliasHolder _aliasHolder;
        
        public Routes(ShellBlueprint blueprint, IAliasHolder aliasHolder) {
            _blueprint = blueprint;
            _aliasHolder = aliasHolder;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            var distinctAreaNames = _blueprint.Controllers
                .Select(controllerBlueprint => controllerBlueprint.AreaName)
                .Distinct();

            return distinctAreaNames.Select(areaName =>
                new RouteDescriptor {
                    Priority = 80,
                    Route = new AliasRoute(_aliasHolder, areaName, new MvcRouteHandler())
                }).ToList();
        }
    }
}