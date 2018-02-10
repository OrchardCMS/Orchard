using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.OpenId.Models;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Azure.Authentication.Services.Twitter {
    [OrchardFeature("Orchard.OpenId.Twitter")]
    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly UrlHelper _urlHelper;

        public MissingSettingsBanner(IOrchardServices orchardServices, UrlHelper urlHelper)
        {
            _orchardServices = orchardServices;
            _urlHelper = urlHelper;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var workContext = _orchardServices.WorkContext;
            var settings = workContext.CurrentSite.As<TwitterSettingsPart>();

            if (settings == null || !settings.IsValid()) {
                var url = _urlHelper.Action("OpenId", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">Twitter settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
