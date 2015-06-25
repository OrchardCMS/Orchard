using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class QueryStringOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider
    {
        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings) {
            // Vary by configured query string parameters.
            var queryString = context.RequestContext.HttpContext.Request.QueryString;
            foreach (var key in queryString.AllKeys)
            {
                if (key == null || (settings.VaryByQueryStringParameters != null && !settings.VaryByQueryStringParameters.Contains(key)))
                    continue;
                yield return new KeyValuePair<string, object>(key, queryString[key]);
            }
        }
    }
}