using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Redis.Configuration {
    public class RedisNotificationProvider : INotificationProvider {
        private readonly RedisConnectionProvider _redisConnectionProvider;
        private readonly FeatureManager _featureManager;

        public RedisNotificationProvider(FeatureManager featureManager, RedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            _featureManager = featureManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            string ConnectionStringKey = "";

            var enabledModules = _featureManager.GetEnabledFeatures();
            var verifyModule = enabledModules.Where(f => f.Id == "Orchard.Redis.Caching").FirstOrDefault();

            if (verifyModule != null) {
                ConnectionStringKey = "Orchard.Redis.Cache";
                if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                    yield return new NotifyEntry { Message = T("You need to configure Redis Cache connection string."), Type = NotifyType.Warning };
                }
            }

            verifyModule = enabledModules.Where(f => f.Id == "Orchard.Redis.OutputCache").FirstOrDefault();

            if (verifyModule != null) {
                ConnectionStringKey = "Orchard.Redis.OutputCache";
                if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                    yield return new NotifyEntry { Message = T("You need to configure Redis OutputCache connection string."), Type = NotifyType.Warning };
                }
            }

            verifyModule = enabledModules.Where(f => f.Id == "Orchard.Redis.MessageBus").FirstOrDefault();

            if (verifyModule != null) {
                ConnectionStringKey = "Orchard.Redis.MessageBus";
                if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                    yield return new NotifyEntry { Message = T("You need to configure Redis MessageBus connection string."), Type = NotifyType.Warning };
                }
            }
        }
    }
}
