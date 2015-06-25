using System.Web.Mvc;
using System.Web.Routing;
using Orchard.OutputCache.Models;
using Orchard.UI.Admin;

namespace Orchard.OutputCache.Providers.RequestIsCacheableProvider {
    public class AdminFilterRequestIsCacheableProvider : IRequestIsCacheableProvider {
        public AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings) {
            // Don't cache admin section requests.
            if (AdminFilter.IsApplied(new RequestContext(context.HttpContext, new RouteData()))) {
                return new AbilityToCacheResponse(false, "This request is for an Admin page");
            }

            return null;
        }
    }
}