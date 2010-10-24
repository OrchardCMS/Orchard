using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Messaging.Models;
using Orchard.Localization;
using Orchard.Email.Models;
using Orchard.Settings;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Email.Services {
    public class MissingSettingsBanner: INotificationProvider {

        public MissingSettingsBanner() {
            T = NullLocalizer.Instance;
        }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {

            var smtpSettings = CurrentSite.As<SmtpSettingsPart>();

            if ( smtpSettings == null || !smtpSettings.IsValid() ) {
                yield return new NotifyEntry { Message = T("The SMTP settings needs to be configured." ), Type = NotifyType.Warning};
            }

            var messageSettings = CurrentSite.As<MessageSettingsPart>().Record;

            if ( messageSettings == null || String.IsNullOrWhiteSpace(messageSettings.DefaultChannelService) ) {
                yield return new NotifyEntry { Message = T("The default channel service needs to be configured."), Type = NotifyType.Warning };
            }
        }
    }
}
