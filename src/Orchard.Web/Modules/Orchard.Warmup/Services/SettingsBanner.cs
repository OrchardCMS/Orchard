using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Warmup.Services {
    public class SettingsBanner: INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public SettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            if ( string.IsNullOrWhiteSpace(_orchardServices.WorkContext.CurrentSite.BaseUrl)) {
                yield return new NotifyEntry { Message = T("The Warmup feature needs the Base Url site setting to be set." ), Type = NotifyType.Warning };
            }
        }
    }
}
