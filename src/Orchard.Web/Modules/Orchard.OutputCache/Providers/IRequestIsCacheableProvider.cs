using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public interface IRequestIsCacheableProvider : IDependency {
        IEnumerable<AbilityToCacheResponse> RequestIsCacheable(ActionExecutingContext context, CacheSettings settings);
    }
}