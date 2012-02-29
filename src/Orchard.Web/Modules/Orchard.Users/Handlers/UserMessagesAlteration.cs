using System;
using System.Linq;
using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class UserMessagesAlteration : IMessageEventHandler {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public UserMessagesAlteration(IContentManager contentManager, ISiteService siteService) {
            _contentManager = contentManager;
            _siteService = siteService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Sending(MessageContext context) {
            if (context.MessagePrepared)
                return;

            switch (context.Type) {
                case MessageTypes.Moderation:
                    context.MailMessage.Subject = T("New account").Text;
                    context.MailMessage.Body =
                        T("The user <b>{0}</b> with email <b>{1}</b> has requested a new account. This user won't be able to log while his account has not been approved.",
                            context.Properties["UserName"], context.Properties["Email"]).Text;
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;

                case MessageTypes.Validation:
                    var registeredWebsite = _siteService.GetSiteSettings().As<RegistrationSettingsPart>().ValidateEmailRegisteredWebsite;
                    var contactEmail = _siteService.GetSiteSettings().As<RegistrationSettingsPart>().ValidateEmailContactEMail;
                    context.MailMessage.Subject = T("Verification E-Mail").Text;
                    context.MailMessage.Body =
                        T("Thank you for registering with {0}.<br/><br/><br/><b>Final Step</b><br/>To verify that you own this e-mail address, please click the following link:<br/><a href=\"{1}\">{1}</a><br/><br/><b>Troubleshooting:</b><br/>If clicking on the link above does not work, try the following:<br/><br/>Select and copy the entire link.<br/>Open a browser window and paste the link in the address bar.<br/>Click <b>Go</b> or, on your keyboard, press <b>Enter</b> or <b>Return</b>.",
                            registeredWebsite, context.Properties["ChallengeUrl"]).Text;

                    if (!String.IsNullOrWhiteSpace(contactEmail)) {
                        context.MailMessage.Body +=
                            T("<br/><br/>If you continue to have access problems or want to report other issues, please <a href=\"mailto:{0}\">Contact Us</a>.",
                                contactEmail).Text;
                    }
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;

                case MessageTypes.LostPassword:
                    var recipient = GetRecipient(context);
                    context.MailMessage.Subject = T("Lost password").Text;
                    context.MailMessage.Body =
                        T("Dear {0}, please <a href=\"{1}\">click here</a> to change your password.", recipient.UserName,
                          context.Properties["LostPasswordUrl"]).Text;
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;
            }
        }

        private UserPart GetRecipient(MessageContext context) {
            // we expect a single account to be created
            var contentItem = _contentManager.Get(context.Recipients.Single().Id);
            if (contentItem == null) {
                return null;
            }
            return contentItem.As<UserPart>();
        }

        private static void FormatEmailBody(MessageContext context) {
            context.MailMessage.Body = "<p style=\"font-family:Arial, Helvetica; font-size:10pt;\">" + context.MailMessage.Body;
            context.MailMessage.Body += "</p>";
        }

        public void Sent(MessageContext context) {
        }
    }
}
