using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers
{
    public interface IOutputCacheKeyCompositeProvider : IDependency {
        IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings);
    }
}