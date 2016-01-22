using System;
using NuGet;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Models {
    public class NugetLogger : ILogger {
        private readonly INotifier _notifier;

        public NugetLogger(INotifier notifier) {
            _notifier = notifier;
        }

        public void Log(MessageLevel level, string message, params object[] args) {
            switch ( level ) {
                case MessageLevel.Debug:
                    break;
                case MessageLevel.Info:
                    _notifier.Information(new LocalizedString(String.Format(message, args)));
                    break;
                case MessageLevel.Warning:
                    _notifier.Warning(new LocalizedString(String.Format(message, args)));
                    break;
            }
        }
    }
}