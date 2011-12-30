using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Email.Services
{
    public class MissingSettingsBanner : INotificationProvider
    {
        private readonly IOrchardServices _orchardServices;
        private readonly WorkContext _workContext;

        public MissingSettingsBanner(IOrchardServices orchardServices, IWorkContextAccessor workContextAccessor)
        {
            _orchardServices = orchardServices;
            _workContext = workContextAccessor.GetContext();
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications()
        {

            var smtpSettings = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();

            if (smtpSettings == null || !smtpSettings.IsValid())
            {
                var urlHelper = new UrlHelper(_workContext.HttpContext.Request.RequestContext);
                var url = urlHelper.Action("Email", "Admin", new { Area = "Settings" });
                yield return new NotifyEntry { Message = T("The <a href=\"{0}\">SMTP settings</a> needs to be configured.", url), Type = NotifyType.Warning };
            }
        }
    }
}
