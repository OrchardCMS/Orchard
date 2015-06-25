using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class AuthenticationStateOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider
    {
        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings)
        {
            // Vary by authentication state if configured.
            if (settings.VaryByAuthenticationState) {
                yield return new KeyValuePair<string, object>("auth", context.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant());
            }
        }
    }
}