using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.UI.Notify {
    /// <summary>
    /// Notification manager for UI notifications
    /// </summary>
    /// <remarks>
    /// Where such notifications are displayed depends on the theme used. Default themes contain a 
    /// Messages zone for this.
    /// </remarks>
    public interface INotifier : IDependency {
        /// <summary>
        /// Adds a new UI notification
        /// </summary>
        /// <param name="type">
        /// The type of the notification (notifications with different types can be displayed differently)</param>
        /// <param name="message">A localized message to display</param>
        void Add(NotifyType type, LocalizedString message);

        /// <summary>
        /// Get all notifications added
        /// </summary>
        IEnumerable<NotifyEntry> List();
    }

    public class Notifier : INotifier {
        private readonly IList<NotifyEntry> _entries;

        public Notifier() {
            Logger = NullLogger.Instance;
            _entries = new List<NotifyEntry>();
        }

        public ILogger Logger { get; set; }

        public void Add(NotifyType type, LocalizedString message) {
            Logger.Information("Notification {0} message: {1}", type, message);
            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IEnumerable<NotifyEntry> List() {
            return _entries;
        }
    }
}