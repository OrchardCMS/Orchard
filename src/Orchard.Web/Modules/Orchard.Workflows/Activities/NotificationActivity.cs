using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities {
    public class NotificationActivity : Task {
        private readonly INotifier _notifier;
        private readonly ITokenizer _tokenizer;

        public NotificationActivity(INotifier notifier, ITokenizer tokenizer) {
            _notifier = notifier;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "Notify"; }
        }

        public override LocalizedString Category {
            get { return T("Notification"); }
        }

        public override LocalizedString Description {
            get { return T("Display a message.");  }
        }

        public override string Form {
            get { return "ActivityNotify"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            yield return T("Done");
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var notification = activityContext.GetState<string>("Notification");
            var message = activityContext.GetState<string>("Message");

            var notificationType = (NotifyType)Enum.Parse(typeof(NotifyType), notification);
            _notifier.Add(notificationType, T(message));

            yield return T("Done");
        }
    }
}