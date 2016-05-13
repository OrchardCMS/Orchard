using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;

namespace Orchard.Redis.Configuration {
    public class RedisNotificationProvider : INotificationProvider {
        private readonly RedisConnectionProvider _redisConnectionProvider;
        public const string ConnectionStringKey = "Orchard.Redis.Cache";

        public RedisNotificationProvider(ShellSettings shellSettings, RedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                yield return new NotifyEntry { Message = T("You need to configure Redis connection string."), Type = NotifyType.Warning };
            }
        }
    }
}
