using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Orchard.Localization {
    [OrchardFeature("Orchard.Localization.CutlurePicker")]
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Culture/Change",
                        new RouteValueDictionary {
                            {"area", "Orchard.Localization"},
                            {"controller", "Culture"},
                            {"action", "ChangeCulture"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Localization"}
                        },
                        new MvcRouteHandler())
                },
            };
        }
    }
}