using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public interface IResponseIsCacheableProvider : IDependency {
        IEnumerable<AbilityToCacheResponse> ResponseIsCacheable(ResultExecutedContext context, CacheRouteConfig configuration, CacheSettings settings);
    }
}