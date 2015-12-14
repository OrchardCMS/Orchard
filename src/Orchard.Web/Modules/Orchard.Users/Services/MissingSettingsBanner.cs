using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageChannelManager _messageManager;

        public MissingSettingsBanner(IOrchardServices orchardServices, IMessageChannelManager messageManager) {
            _orchardServices = orchardServices;
            _messageManager = messageManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {

            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();

            if ( registrationSettings != null &&
                    ( registrationSettings.UsersMustValidateEmail ||
                    registrationSettings.NotifyModeration ||
                    registrationSettings.EnableLostPassword ) &&
                null == _messageManager.GetMessageChannel("Email", new Dictionary<string, object> {
                    {"Body", ""}, 
                    {"Subject", "Subject"},
                    {"Recipients", "john.doe@outlook.com"}
                }) ) {
                yield return new NotifyEntry { Message = T("Some Orchard.User settings require an Email channel to be enabled."), Type = NotifyType.Warning };
            }
        }
    }
}
