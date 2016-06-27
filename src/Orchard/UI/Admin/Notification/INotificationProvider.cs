using System.Collections.Generic;
using Orchard.UI.Notify;

namespace Orchard.UI.Admin.Notification {
    public interface INotificationProvider : IDependency {
        /// <summary>
        /// Returns all notifications to display per zone
        /// </summary>
        IEnumerable<NotifyEntry> GetNotifications();
    }
}
