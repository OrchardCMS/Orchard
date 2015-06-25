using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers.RequestIsCacheableProvider {
    public class AuthenticatedRequestIsCacheableProvider : IRequestIsCacheableProvider {
        private readonly WorkContext _workContext;

        public AuthenticatedRequestIsCacheableProvider(IWorkContextAccessor workContextAccessor) {
            _workContext = workContextAccessor.GetContext();
        }

        public AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings) {
            // Ignore authenticated requests unless the setting to cache them is true.
            if (_workContext.CurrentUser != null && !settings.CacheAuthenticatedRequests) {
                return new AbilityToCacheResponse(false, "This is an authenticated request, and this site is configured to not cache authenticated requests");
            }

            return null;
        }
    }
}