using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class ModerationMessageAlteration : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public ModerationMessageAlteration(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Sending(MessageContext context) {
            var contentItem = _contentManager.Get(context.Recipient.Id);
            if ( contentItem == null )
                return;

            var recipient = contentItem.As<UserPart>();
            if ( recipient == null )
                return;

            if ( context.Type == MessageTypes.Moderation ) {
                context.MailMessage.Subject = "User needs moderation";
                context.MailMessage.Body = string.Format("The following user account needs to be moderated: {0}", recipient.UserName);
            }

            if ( context.Type == MessageTypes.Validation ) {
                context.MailMessage.Subject = "User account validation";
                context.MailMessage.Body = string.Format("Dear {0}, please <a href=\"{1}\">click here</a> to validate you email address.", recipient.UserName, context.Properties["ChallengeUrl"]);
            }

        }

        public void Sent(MessageContext context) {
        }
    }
}
