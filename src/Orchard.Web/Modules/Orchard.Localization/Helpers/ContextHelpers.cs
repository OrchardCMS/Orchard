using System.Web;
using Orchard.UI.Admin;

namespace Orchard.Localization {
    public static class ContextHelpers {
        internal static bool IsRequestFrontEnd(HttpContextBase context) {
            return !IsRequestAdmin(context);
        }

        internal static bool IsRequestAdmin(HttpContextBase context) {
            if (AdminFilter.IsApplied(context.Request.RequestContext))
                return true;

            return false;
        }

    }
}