using System;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;

namespace Orchard.Mvc {
    public interface IControllerType {
        Type ControllerType { get; }
    }

    public class OrchardControllerFactory : DefaultControllerFactory {

        bool TryResolve<T>(WorkContext workContext, object serviceKey, out T instance ) {
            if (workContext != null) {
                var key = new KeyedService(serviceKey, typeof (T));
                object value;
                if (workContext.Resolve<ILifetimeScope>().TryResolve(key, out value)) {
                    instance = (T) value;
                    return true;
                }
            }

            instance = default(T);
            return false;
        }

        protected override Type GetControllerType(RequestContext requestContext, string controllerName) {
            var routeData = requestContext.RouteData;

            // Determine the area name for the request, and fall back to stock orchard controllers
            var areaName = GetAreaName(routeData);

            // Service name pattern matches the identification strategy
            var serviceKey = (areaName + "/" + controllerName).ToLowerInvariant();

            // Now that the request container is known - try to resolve the controller information
            Lazy<Meta<IController>> info;
            var workContext = requestContext.GetWorkContext();
            if (TryResolve(workContext, serviceKey, out info)) {
                return (Type) info.Value.Metadata["ControllerType"];
            }

            return null;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, System.Type controllerType) {
            IController controller;
            var workContext = requestContext.GetWorkContext();
            if (TryResolve(workContext, controllerType, out controller)) {
                return controller;
            }

            return null;
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
    }
}