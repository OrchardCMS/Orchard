using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Azure.MediaServices.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Azure.MediaServices.Services.Wams {
    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly UrlHelper _urlHelper;

        public MissingSettingsBanner(IOrchardServices orchardServices, UrlHelper urlHelper) {
            _orchardServices = orchardServices;
            _urlHelper = urlHelper;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var workContext = _orchardServices.WorkContext;
            var settings = workContext.CurrentSite.As<CloudMediaSettingsPart>();

            if (settings == null || !settings.IsValid()) {
                var url = _urlHelper.Action("Index", "Settings", new {area = "Orchard.Azure.MediaServices"});
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Microsoft Azure Media settings</a> are either missing or invalid and need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
