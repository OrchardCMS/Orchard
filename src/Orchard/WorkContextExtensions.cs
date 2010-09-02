using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;

namespace Orchard {
    public static class WorkContextExtensions {
        public static WorkContext GetContext(this IWorkContextAccessor workContextAccessor, ControllerContext controllerContext) {
            return workContextAccessor.GetContext(controllerContext.RequestContext.HttpContext);
        }

        public static WorkContext GetWorkContext(this RequestContext requestContext) {
            if (requestContext == null) {
                return null;
            }

            var routeData = requestContext.RouteData;
            object value;
            if (routeData == null ||
                routeData.DataTokens == null ||
                !routeData.DataTokens.TryGetValue("IWorkContextAccessor", out value) ||
                !(value is IWorkContextAccessor)) {
                return null;
            }

            var workContextAccessor = (IWorkContextAccessor)value;
            return workContextAccessor.GetContext(requestContext.HttpContext);
        }

        public static WorkContext GetWorkContext(this ControllerContext controllerContext) {
            if (controllerContext == null) {
                return null;
            }
            return WorkContextExtensions.GetWorkContext(controllerContext.RequestContext);
        }

        public static IWorkContextScope CreateWorkContextScope(this ILifetimeScope lifetimeScope, HttpContextBase httpContext) {
            return lifetimeScope.Resolve<IWorkContextAccessor>().CreateWorkContextScope(httpContext);
        }

        public static IWorkContextScope CreateWorkContextScope(this ILifetimeScope lifetimeScope) {
            return lifetimeScope.Resolve<IWorkContextAccessor>().CreateWorkContextScope();
        }
    }
}
