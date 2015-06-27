using System;
using System.Web.Mvc;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Providers {
    public class NotificationsResponseIsCacheableProvider : IResponseIsCacheableProvider {
        public AbilityToCacheResponse ResponseIsCacheable(ResultExecutedContext context, CacheRouteConfig configuration, CacheSettings settings) {
            // Don't cache if request created notifications.
            var hasNotifications = !String.IsNullOrEmpty(Convert.ToString(context.Controller.TempData["messages"]));
            if (hasNotifications) {
                return new AbilityToCacheResponse(false, "One or more notifications were created.");
            }

            return null;
        }
    }
}