using System;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using Orchard.Mvc.Extensions;

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
            var areaName = routeData.GetAreaName();

            // Service name pattern matches the identification strategy
            var serviceKey = (areaName + "/" + controllerName).ToLowerInvariant();

            // Now that the request container is known - try to resolve the controller information
            Meta<Lazy<IController>> info;
            var workContext = requestContext.GetWorkContext();
            if (TryResolve(workContext, serviceKey, out info)) {
                return (Type) info.Metadata["ControllerType"];
            }

            // fail as appropriate for MVC's expectations
            return null;
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType) {
            IController controller;
            var workContext = requestContext.GetWorkContext();
            if (TryResolve(workContext, controllerType, out controller)) {
                return controller;
            }

            // fail as appropriate for MVC's expectations
            return null;
        }

    }
}