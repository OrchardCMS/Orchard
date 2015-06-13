using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace IDeliverable.Widgets.Routes
{
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxWidgetRoutes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            yield return new RouteDescriptor
            {
                Name = "AjaxifyContent",
                Priority = 1,
                Route = new Route(
                    url: "AjaxifyContent/{id}",
                    defaults: new RouteValueDictionary {
                        { "action", "Display" },
                        { "controller", "Ajax" },
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