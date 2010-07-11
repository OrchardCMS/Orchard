using System.Collections.Generic;
using System.Linq;
using Orchard.UI.Notify;

namespace Orchard.UI.Admin.Notification {
    public class NotificationManager : INotificationManager {
        private readonly IEnumerable<INotificationProvider> _notificationProviders;

        public NotificationManager(IEnumerable<INotificationProvider> notificationProviders) {
            _notificationProviders = notificationProviders;
        }

        public IEnumerable<NotifyEntry> GetNotifications() {
            return _notificationProviders
                .SelectMany(n => n.GetNotifications());
        }
    }
}
