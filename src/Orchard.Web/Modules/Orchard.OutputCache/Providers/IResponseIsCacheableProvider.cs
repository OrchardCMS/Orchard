using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public interface IResponseIsCacheableProvider : IDependency {
        AbilityToCacheResponse ResponseIsCacheable(ResultExecutedContext context, CacheRouteConfig configuration, CacheSettings settings);
    }
}