using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public interface IRequestIsCacheableProvider : IDependency {
        AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings);
    }

    public class IgnoredUrlsRequestIsCacheableProvider : IRequestIsCacheableProvider {
        public AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings) {

            return null;
        }
    }
}