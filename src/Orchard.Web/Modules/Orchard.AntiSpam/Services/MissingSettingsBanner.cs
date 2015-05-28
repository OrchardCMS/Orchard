using System.Collections.Generic;
using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace Orchard.AntiSpam.Services {
    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var workContext = _orchardServices.WorkContext;
            var RecaptchaSettings = workContext.CurrentSite.As<ReCaptchaSettingsPart>();

            if (RecaptchaSettings.PublicKey == null || RecaptchaSettings.PrivateKey == null) {
                var urlHelper = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Spam", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">ReCaptcha settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
