using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class ModerationMessageAlteration : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public ModerationMessageAlteration(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Sending(MessageContext context) {
            var contentItem = _contentManager.Get(context.Recipient.Id);
            if ( contentItem == null )
                return;

            var recipient = contentItem.As<UserPart>();
            if ( recipient == null )
                return;

            if ( context.Type == MessageTypes.Moderation ) {
                context.MailMessage.Subject = T("User needs moderation").Text;
                context.MailMessage.Body = T("The following user account needs to be moderated: {0}", recipient.UserName).Text;
            }

            if (context.Type == MessageTypes.Validation) {
                context.MailMessage.Subject = T("User account validation").Text;
                context.MailMessage.Body = T("Dear {0}, please <a href=\"{1}\">click here</a> to validate you email address.", recipient.UserName, context.Properties["ChallengeUrl"]).Text;
            }

            if (context.Type == MessageTypes.LostPassword) {
                context.MailMessage.Subject = T("Lost password").Text;
                context.MailMessage.Body = T("Dear {0}, please <a href=\"{1}\">click here</a> to change your password.", recipient.UserName, context.Properties["LostPasswordUrl"]).Text;
            }

        }

        public void Sent(MessageContext context) {
        }
    }
}
