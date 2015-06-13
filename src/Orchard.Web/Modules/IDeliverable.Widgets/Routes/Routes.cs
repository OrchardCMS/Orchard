using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace IDeliverable.Widgets.Routes
{
    [OrchardFeature("IDeliverable.Widgets")]
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            yield return new RouteDescriptor
            {
                Name = "WidgetEditor",
                Priority = 1,
                Route = new Route(
                    url: "Admin/WidgetsContainer/{action}/{id}",
                    defaults: new RouteValueDictionary {
                        { "id", UrlParameter.Optional },
                        { "controller", "WidgetAdmin" },
                        { "area", "IDeliverable.Widgets" },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", "IDeliverable.Widgets" }
                    },
                    routeHandler: new MvcRouteHandler())
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var route in GetRoutes())
            {
                routes.Add(route);
            }
        }
    }
}