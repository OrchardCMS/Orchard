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
            var contentItem = _contentManager.Get(context.Recipient.Id);
            if ( contentItem == null )
                return;

            var recipient = contentItem.As<IUser>();
            if ( recipient == null )
                return;

            context.MailMessage.To.Add(recipient.Email);
        }

        public void Sent(MessageContext context) {
        }
    }
}
