using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Orchard.OutputCache.Helpers;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers.RequestIsCacheableProvider {
    public class OutputCacheAttributeRequestIsCacheableProvider : IRequestIsCacheableProvider {
        public AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings) {
            // Respect OutputCacheAttribute if applied.
            var actionAttributes = context.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var controllerAttributes = context.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var outputCacheAttribute = actionAttributes.Concat(controllerAttributes).Cast<OutputCacheAttribute>().FirstOrDefault();
            if (outputCacheAttribute != null) {
                if (outputCacheAttribute.Duration <= 0 || outputCacheAttribute.NoStore || outputCacheAttribute.LocationIsIn(OutputCacheLocation.Downstream, OutputCacheLocation.Client, OutputCacheLocation.None)) {
                    return new AbilityToCacheResponse(false, "Request ignored based on OutputCache attribute");
                }
            }

            return null;
        }
    }
}