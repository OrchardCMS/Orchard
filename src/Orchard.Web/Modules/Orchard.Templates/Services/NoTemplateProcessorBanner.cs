using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Templates.Services {
    public class NoTemplateProcessorBanner : INotificationProvider {
        private readonly IEnumerable<ITemplateProcessor> _processors;

        public NoTemplateProcessorBanner(IEnumerable<ITemplateProcessor> processors) {
            _processors = processors;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            if (!_processors.Any()) {
                yield return new NotifyEntry { Message = T("To be able to use Templates enable a template processor like Razor Templates."), Type = NotifyType.Warning };
            }
        }
    }
}