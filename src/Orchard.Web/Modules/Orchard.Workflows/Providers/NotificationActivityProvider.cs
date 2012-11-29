using System;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;
using Orchard.Workflows.Models.Descriptors;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Providers {
    public class NotificationActivityProvider : IActivityProvider {
        private readonly INotifier _notifier;
        private readonly ITokenizer _tokenizer;

        public NotificationActivityProvider(INotifier notifier, ITokenizer tokenizer) {
            _notifier = notifier;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActivityContext describe) {
            describe.For("Notification", T("Notification"), T("Notifications"))
                .Element(
                    "Notify",
                    T("Notify"),
                    T("Display a message."),
                    ExecuteActivity,
                    DisplayActivity,
                    "ActivityNotify"
                );
        }

        private bool ExecuteActivity(ActivityContext context) {
            string notification = context.State.Notification;
            string message = context.State.Message;

            message = _tokenizer.Replace(message, context.Tokens);

            var notificationType = (NotifyType)Enum.Parse(typeof(NotifyType), notification);
            _notifier.Add(notificationType, T(message));

            return true;
        }

        private LocalizedString DisplayActivity(ActivityContext context) {
            return T("Displays \"{1}\" as {0}", T(context.State.Notification).Text, T(context.State.Message).Text);
        }
    }
}