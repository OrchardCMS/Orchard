using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Core.Common.Models;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {

    public class MailActivity : Task {
        private readonly IMessageManager _messageManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        public const string MessageType = "ActionEmail";

        public MailActivity(
            IMessageManager messageManager,
            IOrchardServices orchardServices,
            IMembershipService membershipService) {
            _messageManager = messageManager;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Sent") };
        }

        public override string Form {
            get {
                return "ActivityActionEmail";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendEmail"; }
        }


        public override LocalizedString Description {
            get { return T("Sends an e-mail to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string recipient = activityContext.GetState<string>("Recipient");

            var properties = new Dictionary<string, string> {
                {"Body", activityContext.GetState<string>("Body")}, 
                {"Subject", activityContext.GetState<string>("Subject")},
                {"RecipientOther",activityContext.GetState<string>("RecipientOther")}
            }; 

            if (recipient == "owner") {
                var content = workflowContext.Content;
                if (content.Has<CommonPart>()) {
                    var owner = content.As<CommonPart>().Owner;
                    if (owner != null && owner.ContentItem != null && owner.ContentItem.Record != null) {
                        _messageManager.Send(owner.ContentItem.Record, MessageType, "email", properties);
                    }
                    _messageManager.Send(
                        SplitEmail(owner.As<IUser>().Email), MessageType, "email", properties);
                }
            }
            else if (recipient == "author") {
                var user = _orchardServices.WorkContext.CurrentUser;

                // can be null if user is anonymous
                if (user != null && String.IsNullOrWhiteSpace(user.Email)) {
                    _messageManager.Send(user.ContentItem.Record, MessageType, "email", properties);
                }
            }
            else if (recipient == "admin") {
                var username = _orchardServices.WorkContext.CurrentSite.SuperUser;
                var user = _membershipService.GetUser(username);

                // can be null if user is no super user is defined
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    _messageManager.Send(user.ContentItem.Record, MessageType, "email", properties);
                }
            }
            else if (recipient == "other") {
                _messageManager.Send(SplitEmail(activityContext.GetState<string>("RecipientOther")), MessageType, "email", properties);
            }

            yield return T("Sent");
        }

        private static IEnumerable<string> SplitEmail(string commaSeparated) {
            return commaSeparated.Split(new[] { ',', ';' });
        }
    }

    public class MailActionsHandler : IMessageEventHandler {
        public MailActionsHandler() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Sending(MessageContext context) {
            if (context.MessagePrepared)
                return;

            if ((context.Recipients == null || !context.Recipients.Any()) &&
                (context.Addresses == null || !context.Addresses.Any())) {
                return;
            }

            switch (context.Type) {
                case MailActivity.MessageType:
                    context.MailMessage.Subject = context.Properties["Subject"];
                    context.MailMessage.Body = context.Properties["Body"];
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;
            }
        }

        private static void FormatEmailBody(MessageContext context) {
            context.MailMessage.Body = "<p style=\"font-family:Arial, Helvetica; font-size:10pt;\">" + context.MailMessage.Body + "</p>";
        }

        public void Sent(MessageContext context) {
        }
    }
}