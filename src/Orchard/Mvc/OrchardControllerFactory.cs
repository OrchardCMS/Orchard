using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Web;

namespace Orchard.Mvc {
    public class OrchardControllerFactory : DefaultControllerFactory {

        public override IController CreateController(RequestContext requestContext, string controllerName) {
            var routeData = requestContext.RouteData;

            // Determine the area name for the request, and fall back to stock orchard controllers
            var areaName = GetAreaName(routeData);

            // Service name pattern matches the identification strategy
            var serviceKey = (areaName + "/" + controllerName).ToLowerInvariant();

            // Now that the request container is known - try to resolve the controller            
            object controller;
            var service = new KeyedService(serviceKey, typeof(IController));

            // Locate the container this route is bound against
            var workContextAccessor = GetWorkContextAccessor(routeData);

            var workContext = workContextAccessor != null ? workContextAccessor.GetContext(requestContext.HttpContext) : null;

            if (workContext != null &&
                workContext.Service<ILifetimeScope>().TryResolve(service, out controller)) {
                return (IController)controller;
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

        static IWorkContextAccessor GetWorkContextAccessor(RouteData routeData) {
            object dataTokenValue;
            if (routeData != null &&
                routeData.DataTokens != null &&
                routeData.DataTokens.TryGetValue("IWorkContextAccessor", out dataTokenValue) &&
                dataTokenValue is IWorkContextAccessor) {
                return (IWorkContextAccessor)dataTokenValue;
            }
            return null;
        }
    }
}