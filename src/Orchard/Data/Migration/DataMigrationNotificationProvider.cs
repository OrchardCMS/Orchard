using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Data.Migration {
    public class DataMigrationNotificationProvider: INotificationProvider {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        public const string CacheKey = "DataMigrationNotifications";
        public const string SignalKey = "DataMigrationNotificationsChanged";

        public DataMigrationNotificationProvider(IDataMigrationManager dataMigrationManager, ICacheManager cacheManager, ISignals signals) {
            _dataMigrationManager = dataMigrationManager;
            _cacheManager = cacheManager;
            _signals = signals;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var features = _cacheManager.Get(CacheKey, ctx => {
                                                           ctx.Monitor(_signals.When(SignalKey));
                                                           return _dataMigrationManager.GetFeaturesThatNeedUpdate();
                                                       });

            if(features.Any()) {
                yield return new NotifyEntry { Message = T("Some features need to be upgraded: {0}", String.Join(", ", features)), Type = NotifyType.Warning};
            }
        }
    }
}
