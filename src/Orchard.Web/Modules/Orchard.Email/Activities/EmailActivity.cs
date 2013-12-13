using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Email.Activities {
    public class EmailActivity : Task {
        private readonly IMessageQueueManager _messageQueueManager;

        public EmailActivity(IMessageQueueManager messageQueueManager) {
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
            var recipientAddresses = Split(activityContext.GetState<string>("RecipientAddress")).ToList();
            var body = activityContext.GetState<string>("Body");
            var subject = activityContext.GetState<string>("Subject");
            var queueId = activityContext.GetState<int?>("Queue") ?? _messageQueueManager.GetDefaultQueue().Id;
            var priorityId = activityContext.GetState<int>("Priority");
            var recipients = recipientAddresses.Select(x => new MessageRecipient(x));
            var priority = _messageQueueManager.GetPriority(priorityId);
            var payload = new EmailMessage(subject, body);
            _messageQueueManager.Send(recipients, EmailMessageChannel.ChannelName, payload, priority, queueId);

            yield return T("Queued");
        }

        private static IEnumerable<string> Split(string value) {
            return !String.IsNullOrWhiteSpace(value) ? value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
        }
    }
}