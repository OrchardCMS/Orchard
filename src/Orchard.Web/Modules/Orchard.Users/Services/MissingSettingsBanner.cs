using System.Linq;
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
        private readonly IMessageManager _messageManager;

        public MissingSettingsBanner(IOrchardServices orchardServices, IMessageManager messageManager) {
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
                !_messageManager.GetAvailableChannelServices().Contains("email") ) {
                yield return new NotifyEntry { Message = T("Some Orchard.User settings require an Email channel to be enabled."), Type = NotifyType.Warning };
            }
        }
    }
}
