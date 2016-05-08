using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.MessageBus.Services;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Orchard.Redis.Configuration {
    public class RedisNotificationProvider : INotificationProvider {
        private readonly ShellSettings _shellSettings;

        public RedisNotificationProvider(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var _defaultSettingsKey = "Orchard.Redis.MessageBus";
            var _tenantSettingsKey = _shellSettings.Name + ":" + _defaultSettingsKey;

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[_tenantSettingsKey] ?? ConfigurationManager.ConnectionStrings[_defaultSettingsKey];

            if (connectionStringSettings == null) {
                yield return new NotifyEntry { Message = T("You need to configure Redis connection string."), Type = NotifyType.Warning };
            }
        }
    }
}
