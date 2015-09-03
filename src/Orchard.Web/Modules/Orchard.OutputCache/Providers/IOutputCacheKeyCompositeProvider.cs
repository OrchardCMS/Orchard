using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers
{
    /// <summary>
    /// Provides the ability add custom logic to append values to the Output Cache key
    /// </summary>
    public interface IOutputCacheKeyCompositeProvider : IDependency {
        /// <summary>
        /// Gets a collection of values to be appended to the Output Cache key
        /// </summary>
        /// <param name="context">The context from the current request</param>
        /// <param name="settings">The current configuration values for this site's Output Cache settings</param>
        /// <returns>A collection of key value pairs that contains the values that shgould be appended to the Output Cache key</returns>
        IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings);
    }
}