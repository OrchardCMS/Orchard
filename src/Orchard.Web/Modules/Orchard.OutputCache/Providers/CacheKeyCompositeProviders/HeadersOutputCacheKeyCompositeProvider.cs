using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class HeadersOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider {
        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings) {
            // Vary by configured request headers.
            var requestHeaders = context.RequestContext.HttpContext.Request.Headers;
            foreach (var varyByRequestHeader in settings.VaryByRequestHeaders) {
                if (requestHeaders.AllKeys.Contains(varyByRequestHeader))
                    yield return new KeyValuePair<string, object>("HEADER:" + varyByRequestHeader, requestHeaders[varyByRequestHeader]);
            }
        }
    }
}