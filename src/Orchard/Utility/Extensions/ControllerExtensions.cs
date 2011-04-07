using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

namespace Orchard.Utility.Extensions {
    public static class ControllerExtensions {
        public static void Error(this Controller controller,
            Exception exception,
            LocalizedString localizedString,
            ILogger logger,
            INotifier notifier) {

            logger.Error(exception, localizedString.ToString());
            notifier.Error(localizedString);

            for (Exception innerException = exception; innerException != null ; innerException = innerException.InnerException) {
                notifier.Error(new LocalizedString(innerException.Message));
            }
        }
    }
}