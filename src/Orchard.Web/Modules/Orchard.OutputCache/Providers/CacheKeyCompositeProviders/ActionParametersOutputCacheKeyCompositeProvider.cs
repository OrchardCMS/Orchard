using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers.CacheKeyCompositeProviders {
    public class ActionParametersOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider {
        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings)
        {
            // Vary by action parameters.
            foreach (var p in context.ActionParameters)
                yield return new KeyValuePair<string, object>("PARAM:" + p.Key, p.Value);
        }
    }
}