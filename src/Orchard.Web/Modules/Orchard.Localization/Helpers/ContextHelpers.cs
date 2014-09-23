using System.Web;
using System.Web.Routing;
using Orchard.UI.Admin;

namespace Orchard.Localization {
    public static class ContextHelpers {
        internal static bool IsRequestFrontEnd(HttpContextBase context) {
            return !IsRequestAdmin(context);
        }

        internal static bool IsRequestAdmin(HttpContextBase context) {
            if (AdminFilter.IsApplied(new RequestContext(context, new RouteData())))
                return true;

            return false;
        }

    }
}