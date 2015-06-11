using Orchard.Localization;

namespace Orchard.UI.Notify {
    public static class NotifierExtensions {
        /// <summary>
        /// Adds a new UI notification of type Information
        /// </summary>
        /// <seealso cref="Orchard.UI.Notify.INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Information(this INotifier notifier, LocalizedString message) {
            notifier.Add(NotifyType.Information, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Warning
        /// </summary>
        /// <seealso cref="Orchard.UI.Notify.INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Warning(this INotifier notifier, LocalizedString message) {
            notifier.Add(NotifyType.Warning, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Error
        /// </summary>
        /// <seealso cref="Orchard.UI.Notify.INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Error(this INotifier notifier, LocalizedString message) {
            notifier.Add(NotifyType.Error, message);
        }
    }
}