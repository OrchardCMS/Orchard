using System.Web.Mvc;

namespace Orchard {
    public static class WorkContextExtensions {
        public static WorkContext GetContext(this IWorkContextAccessor workContextAccessor, ControllerContext controllerContext) {
            return workContextAccessor.GetContext(controllerContext.RequestContext.HttpContext);
        }
    }
}
