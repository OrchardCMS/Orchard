using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Email.Services {
    public class MissingSettingsBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;

        public MissingSettingsBanner(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var workContext = _orchardServices.WorkContext;
            var smtpSettings = workContext.CurrentSite.As<SmtpSettingsPart>();

            if (smtpSettings == null || !smtpSettings.IsValid()) {
                var urlHelper = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Email", "Admin", new {Area = "Settings"});
                yield return new NotifyEntry {Message = T("The <a href=\"{0}\">SMTP settings</a> need to be configured.", url), Type = NotifyType.Warning};
            }
        }
    }
}
