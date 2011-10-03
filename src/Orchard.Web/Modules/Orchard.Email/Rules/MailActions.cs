using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Email.Rules {

    public interface IActionProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    [OrchardFeature("Orchard.Email.Rules")]
    public class MailActions : IActionProvider {
        private readonly IMessageManager _messageManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        public const string MessageType = "ActionEmail";

        public MailActions(
            IMessageManager messageManager,
            IOrchardServices orchardServices,
            IMembershipService membershipService) {
            _messageManager = messageManager;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            Func<dynamic, LocalizedString> display = context => T("Send an e-mail");

            describe.For("Messaging", T("Messaging"), T("Messages"))
                .Element("SendEmail", T("Send e-mail"), T("Sends an e-mail to a specific user."), (Func<dynamic, bool>)Send, display, "ActionEmail");
        }

        private bool Send(dynamic context) {
            var recipient = context.Properties["Recipient"];
            ContentItemRecord recipientRecord = null;

            if (recipient == "owner") {
                var content = context.Tokens["Content"] as IContent;
                if (content.Has<CommonPart>()) {
                    recipientRecord = content.As<CommonPart>().Owner.ContentItem.Record;
                }
            }

            if (recipient == "author") {
                var user = _orchardServices.WorkContext.CurrentUser;

                // can be null if user is anonymous
                if (user != null && String.IsNullOrWhiteSpace(user.Email)) {
                    recipientRecord = user.ContentItem.Record;
                }
            }

            if (recipient == "admin") {
                var username = _orchardServices.WorkContext.CurrentSite.SuperUser;
                var user = _membershipService.GetUser(username);

                // can be null if user is no super user is defined
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    recipientRecord = user.ContentItem.Record;
                }
            }

            if (recipientRecord == null) {
                return true;
            }

            var properties = new Dictionary<string, string>(context.Properties);

            _messageManager.Send(recipientRecord, MessageType, "email", properties);

            return true;
        }
    }

    public class MailActionsHandler : IMessageEventHandler {
        private readonly IContentManager _contentManager;

        public MailActionsHandler(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Sending(MessageContext context) {
            if (context.MessagePrepared)
                return;

            var contentItem = _contentManager.Get(context.Recipient.Id);
            if (contentItem == null)
                return;

            var recipient = contentItem.As<IUser>();
            if (recipient == null)
                return;

            switch (context.Type) {
                case MailActions.MessageType:
                    context.MailMessage.Subject = context.Properties["Subject"];
                    context.MailMessage.Body = context.Properties["Body"];
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;
            }
        }

        private static void FormatEmailBody(MessageContext context) {
            context.MailMessage.Body = "<p style=\"font-family:Arial, Helvetica; font-size:10pt;\">" + context.MailMessage.Body;
            context.MailMessage.Body += "</p>";
        }

        public void Sent(MessageContext context) {
        }
    }

}