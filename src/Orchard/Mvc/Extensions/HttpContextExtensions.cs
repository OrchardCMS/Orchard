using System.Web;

namespace Orchard.Mvc.Extensions {
    public static class HttpContextExtensions {
        public static bool IsBackgroundContext(this HttpContextBase httpContext) {
            return httpContext == null || httpContext is MvcModule.HttpContextPlaceholder;
        }

        public static bool IsBackgroundContext(this HttpContext httpContext) {
            return httpContext == null || httpContext.Items.Contains(StaticHttpContextScopeFactory.IsBackgroundHttpContextKey);
        }
    }
}
