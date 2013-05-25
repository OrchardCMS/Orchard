using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Orchard.OutputCache.ViewModels;
using Orchard;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Services {
    public interface ICacheService : IDependency {
        /// <summary>
        /// Returns the parameters for a specific route
        /// </summary>
        /// <param name="key">The key representing the route</param>
        /// <returns>A <see cref="CacheParameterRecord"/> instance for the specified key, or <c>null</c></returns>
        CacheParameterRecord GetCacheParameterByKey(string key);

        /// <summary>
        /// Removes all cache entries associated with a specific tag.
        /// </summary>
        /// <param name="tag">The tag value.</param>
        void RemoveByTag(string tag);

        /// <summary>
        /// Returns the key representing a specific route in the db
        /// </summary>
        string GetRouteDescriptorKey(HttpContextBase httpContext, RouteBase route);

        /// <summary>
        /// Saves a set of <see cref="RouteConfiguration"/> to the database
        /// </summary>
        /// <param name="routeConfigurations"></param>
        void SaveCacheConfigurations(IEnumerable<RouteConfiguration> routeConfigurations);

        /// <summary>
        /// Returns all defined configurations for specific routes
        /// </summary>
        IEnumerable<RouteConfiguration> GetRouteConfigurations();
    }
}