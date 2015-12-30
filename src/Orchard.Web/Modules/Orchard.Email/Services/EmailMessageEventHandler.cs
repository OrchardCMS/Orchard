using System;
using Orchard.Logging;
using Orchard.Messaging.Events;
using Orchard.ContentManagement;
using Orchard.Messaging.Models;
using Orchard.Security;

namespace Orchard.Email.Services {
    [Obsolete]
    public class EmailMessageEventHandler : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public EmailMessageEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sending(MessageContext context) {
            if (context.Recipients != null) {
                foreach (var rec in context.Recipients) {
                    var contentItem = _contentManager.Get(rec.Id);
                    if (contentItem == null)
                        return;

                    var recipient = contentItem.As<IUser>();
                    if (recipient == null)
                        return;

                    context.MailMessage.To.Add(recipient.Email);
                }
            }

            foreach (var address in context.Addresses) {
                try {
                    context.MailMessage.To.Add(address);
                }
                catch (Exception e) {
                    Logger.Error(e, "Unexpected error while trying to send email.");
                }
            }
        }

        public void Sent(MessageContext context) {
        }
    }
}
