using System;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers.RequestIsCacheableProviders {
    public class PostRequestIsCacheableProvider : IRequestIsCacheableProvider {
        public AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings) {
            // Don't cache POST requests.
            if (context.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                return new AbilityToCacheResponse(false, "The HTTP method for this request is POST");
            }

            return null;
        }
    }
}