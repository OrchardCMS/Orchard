﻿using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Redis.Configuration {
    public class RedisOutputCacheNotificationProvider : INotificationProvider {
        private readonly RedisConnectionProvider _redisConnectionProvider;
        private readonly FeatureManager _featureManager;

        public RedisOutputCacheNotificationProvider(FeatureManager featureManager, RedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            _featureManager = featureManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            //verify if module is enabled first
            if (!_featureManager.GetEnabledFeatures().Where(f => f.Id == "Orchard.Redis.OutputCache").Any()) {
                yield break;
            }

            //verify if there is a connection string set in the web.config
            string ConnectionStringKey = "Orchard.Redis.OutputCache";

            if (_redisConnectionProvider.GetConnectionString(ConnectionStringKey) == null) {
                yield return new NotifyEntry { Message = T("You need to configure Redis OutputCache connection string."), Type = NotifyType.Warning };
            }
        }
    }
}
