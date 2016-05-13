using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;

namespace Orchard.Redis.Configuration {

    [OrchardFeature("Orchard.Redis.Caching")]
    public class RedisCacheNotificationProvider : INotificationProvider {
        private readonly RedisConnectionProvider _redisConnectionProvider;

        public RedisCacheNotificationProvider(RedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            //verify if there is a connection string set in the web.config
            string ConnectionStringKey = "Orchard.Redis.Cache";

            if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                yield return new NotifyEntry { Message = T("You need to configure Redis Cache connection string."), Type = NotifyType.Warning };
            }
        }
    }
}
