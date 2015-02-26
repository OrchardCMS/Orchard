using System.Globalization;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;

namespace Orchard.Localization.Selectors {
    [OrchardFeature("Orchard.Localization.CultureSelector")]
    public class RouteCultureSelector : ICultureSelector {
        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (context == null || ContextHelpers.IsRequestAdmin(context)) return null;

            // Attempt to determine culture by route.
            // This normally happens when you are using non standard pages that are not content items
            // {culture}/foo or ?culture={culture}
            var routeCulture = context.Request.RequestContext.RouteData.Values["culture"] ??
                context.Request.RequestContext.HttpContext.Request.Params["culture"];
            if (routeCulture != null && !string.IsNullOrWhiteSpace(routeCulture.ToString())) {
                try {
                    var culture = CultureInfo.GetCultureInfo(routeCulture.ToString());
                    return new CultureSelectorResult {Priority = -3, CultureName = culture.Name};
                }
                catch {}
            }

            return null;
        }
    }
}