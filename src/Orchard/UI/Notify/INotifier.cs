using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.UI.Notify {
    public interface INotifier : IDependency {
        void Information(LocalizedString message);
        void Warning(LocalizedString message);
        void Error(LocalizedString message);

        void Add(NotifyType type, LocalizedString message);
        IEnumerable<NotifyEntry> List();
    }
}