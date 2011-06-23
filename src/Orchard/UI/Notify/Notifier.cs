using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.UI.Notify {
    public class Notifier : INotifier {
        private readonly IList<NotifyEntry> _entries;

        public Notifier() {
            Logger = NullLogger.Instance;
            _entries = new List<NotifyEntry>();
        }

        public ILogger Logger { get; set; }

        public void Information(LocalizedString message) {
            Add(NotifyType.Information, message);
        }

        public void Warning(LocalizedString message) {
            Add(NotifyType.Warning, message);
        }

        public void Error(LocalizedString message) {
            Add(NotifyType.Error, message);
        }

        public void Add(NotifyType type, LocalizedString message) {
            Logger.Information("Notification {0} message: {1}", type, message);
            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IEnumerable<NotifyEntry> List() {
            return _entries;
        }
    }
}