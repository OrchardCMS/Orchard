using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Filters;

namespace Orchard.Themes {
    public class ThemeFilter : FilterProvider, IActionFilter, IResultFilter {
        public void OnActionExecuting(ActionExecutingContext filterContext) {
            var attribute = GetThemedAttribute(filterContext.ActionDescriptor);
            if (attribute != null && attribute.Enabled) {
                Apply(filterContext.RequestContext);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {}

        public void OnResultExecuting(ResultExecutingContext filterContext) {
#if REFACTORING
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            Apply(filterContext.RequestContext);
#endif
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}

        public static void Apply(RequestContext context) {
            // the value isn't important
            context.HttpContext.Items[typeof (ThemeFilter)] = null;
        }

        public static bool IsApplied(RequestContext context) {
            return context.HttpContext.Items.Contains(typeof (ThemeFilter));
        }

        private static ThemedAttribute GetThemedAttribute(ActionDescriptor descriptor) {
            return descriptor.GetCustomAttributes(typeof (ThemedAttribute), true)
                .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof (ThemedAttribute), true))
                .OfType<ThemedAttribute>()
                .FirstOrDefault();
        }
    }
}