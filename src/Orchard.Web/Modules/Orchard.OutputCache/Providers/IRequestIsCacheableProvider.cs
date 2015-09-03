using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    /// <summary>
    /// Provides a means to allow for custom logic to be executed when determining whether or not a request can be Output Cached
    /// </summary>
    public interface IRequestIsCacheableProvider : IDependency {
        /// <summary>
        /// Executes the logic to determine whether or not this request can be cached
        /// </summary>
        /// <param name="context">The context from the current request</param>
        /// <param name="settings">The current configuration values for this site's Output Cache settings</param>
        /// <returns>An instance of <see cref="AbilityToCacheResponse"/> that contains a flag indicating if the request can be cached, along with an option message. This method can return null, which indidates that this provider has chosen not to provide any logic as to whther or not this request can be cached. </returns>
        AbilityToCacheResponse RequestIsCacheable(ActionExecutingContext context, CacheSettings settings);
    }
}