using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.UI.Notify {
    public interface INotifier : IDependency {
        void Add(NotifyType type, LocalizedString message);
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