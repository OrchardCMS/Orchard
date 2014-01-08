using System;
using System.Collections.Generic;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Email.Activities {
    public class EmailActivity : Task {
        private readonly IMessageQueueService _messageQueueManager;

        public EmailActivity(IMessageQueueService messageQueueManager) {
            _messageQueueManager = messageQueueManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Queued") };
        }

        public override string Form {
            get {
                return "EmailActivity";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendEmail"; }
        }

        public override LocalizedString Description {
            get { return T("Sends an email to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var priority = activityContext.GetState<int>("Priority");

            var body = activityContext.GetState<string>("Body");
            var subject = activityContext.GetState<string>("Subject");
            var recipients = Split(activityContext.GetState<string>("RecipientAddress"));
            var payload = new EmailMessage {
                Subject = subject, 
                Body = body,
                Recipients = recipients
            };

            _messageQueueManager.Enqueue(SmtpMessageChannel.MessageType, payload, priority);

            yield return T("Queued");
        }

        private static string[] Split(string value) {
            return !String.IsNullOrWhiteSpace(value) ? value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
        }
    }
}