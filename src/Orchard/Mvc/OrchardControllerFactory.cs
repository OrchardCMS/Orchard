using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;

namespace Orchard.Mvc {
    public class OrchardControllerFactory : DefaultControllerFactory {
        public override IController CreateController(RequestContext requestContext, string controllerName) {
            var routeData = requestContext.RouteData;

            // Locate the container this route is bound against
            var container = GetRequestContainer(routeData);

            // Determine the area name for the request, and fall back to stock orchard controllers
            var areaName = GetAreaName(routeData) ?? "Orchard";

            // Service name pattern matches the identification strategy
            var serviceName = ("controller." + areaName + "." + controllerName).ToLowerInvariant();

            // Now that the request container is known - try to resolve the controller            
            object controller;
            if (container != null &&
                container.TryResolve(serviceName, out controller, TypedParameter.From(requestContext))) {
                return (IController) controller;
            }
            return base.CreateController(requestContext, controllerName);
        }

        public static string GetAreaName(RouteBase route) {
            var routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null) {
                return routeWithArea.Area;
            }

            var castRoute = route as Route;
            if (castRoute != null && castRoute.DataTokens != null) {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }

        public static string GetAreaName(RouteData routeData) {
            object area;
            if (routeData.DataTokens.TryGetValue("area", out area)) {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }

        public static IContainer GetRequestContainer(RouteData routeData) {
            object dataTokenValue;
            if (routeData != null &&
                routeData.DataTokens != null &&
                routeData.DataTokens.TryGetValue("IContainerProvider", out dataTokenValue) &&
                dataTokenValue is IContainerProvider) {
                var containerProvider = (IContainerProvider) dataTokenValue;
                return containerProvider.RequestContainer;
            }
            return null;
        }
    }
}