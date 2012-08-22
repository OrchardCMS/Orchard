using System;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.Localization;
using Orchard.Tokens;
using Orchard.UI.Notify;

namespace Orchard.Rules.Providers {
    public class NotificationActions : IActionProvider {
        private readonly INotifier _notifier;
        private readonly ITokenizer _tokenizer;

        public NotificationActions(INotifier notifier, ITokenizer tokenizer) {
            _notifier = notifier;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeActionContext describe) {
            describe.For("Notification", T("Notification"), T("Notifications"))
                .Element(
                    "Notify",
                    T("Notify"),
                    T("Display a message."),
                    context => {
                        var notification = context.Properties["notification"];
                        var message = context.Properties["message"];

                        message = _tokenizer.Replace(message, context.Tokens);

                        var notificationType = (NotifyType)Enum.Parse(typeof(NotifyType), notification);
                        _notifier.Add(notificationType, T(message));

                        return true;
                    },
                    context => T("Displays \"{1}\" as {0}", T(context.Properties["notification"]).Text, T(context.Properties["message"]).Text),
                    "ActionNotify");
        }
    }
}