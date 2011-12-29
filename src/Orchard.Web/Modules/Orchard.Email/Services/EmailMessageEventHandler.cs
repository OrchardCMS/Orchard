using Orchard.Messaging.Events;
using Orchard.ContentManagement;
using Orchard.Messaging.Models;
using Orchard.Security;

namespace Orchard.Email.Services {
    public class EmailMessageEventHandler : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public EmailMessageEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

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
                context.MailMessage.To.Add(address);
            }
        }

        public void Sent(MessageContext context) {
        }
    }
}
