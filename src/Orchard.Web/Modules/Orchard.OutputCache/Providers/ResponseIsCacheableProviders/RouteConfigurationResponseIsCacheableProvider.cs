using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class RouteConfigurationResponseIsCacheableProvider : IResponseIsCacheableProvider {
        public AbilityToCacheResponse ResponseIsCacheable(ResultExecutedContext context, CacheRouteConfig configuration, CacheSettings settings) {
            // Don't cache if individual route configuration says so.
            if (configuration != null && configuration.Duration == 0) {
                return new AbilityToCacheResponse(false, "This route is configured to not be cached.");
            }

            return null;
        }
    }
}