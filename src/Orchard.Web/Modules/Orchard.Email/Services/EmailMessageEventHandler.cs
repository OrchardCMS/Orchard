using Orchard.Messaging.Events;
using Orchard.Core.Messaging.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Messaging.Models;

namespace Orchard.Email.Services {
    public class EmailMessageEventHandler : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public EmailMessageEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Sending(MessageContext context) {
            var contentItem = _contentManager.Get(context.Message.Recipient.Id);
            if ( contentItem == null )
                return;

            var recipient = contentItem.As<UserPart>();
            if ( recipient == null )
                return;

            context.Properties.Add(EmailMessagingChannel.EmailAddress, recipient.Email);
        }

        public void Sent(MessageContext context) {
        }
    }
}
