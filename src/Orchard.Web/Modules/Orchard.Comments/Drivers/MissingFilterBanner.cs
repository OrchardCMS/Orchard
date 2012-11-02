using System.Collections.Generic;
using System.Dynamic;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace Orchard.Comments.Drivers {
    public class MissingFilterBanner : INotificationProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly ICheckSpamEventHandler _spamEventHandler;

        public MissingFilterBanner(IOrchardServices orchardServices, ICheckSpamEventHandler spamEventHandler) {
            _orchardServices = orchardServices;
            _spamEventHandler = spamEventHandler;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<NotifyEntry> GetNotifications() {

            var commentSettings = _orchardServices.WorkContext.CurrentSite.As<CommentSettingsPart>();

            dynamic context = new ExpandoObject();
            context.Checked = false;
            context.IsSpam = false;
            context.Text = string.Empty;
            
            _spamEventHandler.CheckSpam(context);

            if (commentSettings != null && commentSettings.EnableSpamProtection && !context.Checked) {
                yield return new NotifyEntry {Message = T("Comments anti-spam protection requires at least one anti-spam filter to be enabled and configured."), Type = NotifyType.Warning};
            }
        }
    }
}
