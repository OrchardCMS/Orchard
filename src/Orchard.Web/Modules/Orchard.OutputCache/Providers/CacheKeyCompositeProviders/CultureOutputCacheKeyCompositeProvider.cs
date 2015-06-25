using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class CultureOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider
    {
        private readonly WorkContext _workContext;

        public CultureOutputCacheKeyCompositeProvider(IWorkContextAccessor workContext) {
            _workContext = workContext.GetContext();
        }

        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings)
        {
            // Vary by request culture if configured.
            if (_workContext != null && settings.VaryByCulture) {
                yield return new KeyValuePair<string, object>("culture", _workContext.CurrentCulture.ToLowerInvariant());
            }
        }
    }
}