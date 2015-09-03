using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public interface IResponseIsCacheableProvider : IDependency {
        /// <summary>
        /// Executes the logic to determine whether or not this response can be cached
        /// </summary>
        /// <param name="context">The context from the current request</param>
        /// <param name="configuration">The configuration for the current route's Output Cache settings</param>
        /// <param name="settings">The current configuration values for this site's Output Cache settings</param>
        /// <returns>An instance of <see cref="AbilityToCacheResponse"/> that contains a flag indicating if the request can be cached, along with an option message. This method can return null, which indidates that this provider has chosen not to provide any logic as to whther or not this response can be cached. </returns>
        AbilityToCacheResponse ResponseIsCacheable(ResultExecutedContext context, CacheRouteConfig configuration, CacheSettings settings);
    }
}