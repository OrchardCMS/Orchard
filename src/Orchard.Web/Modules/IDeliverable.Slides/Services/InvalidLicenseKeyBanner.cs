using System.Collections.Generic;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Services
{
    public class InvalidLicenseKeyBanner : Component, INotificationProvider
    {
        private readonly IOrchardServices _services;

        public InvalidLicenseKeyBanner(IOrchardServices services)
        {
            _services = services;
        }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var workContext = _services.WorkContext;

            if (smtpSettings == null || !smtpSettings.IsValid())
            {
                var urlHelper = new UrlHelper(workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Email", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">SMTP settings</a> need to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
