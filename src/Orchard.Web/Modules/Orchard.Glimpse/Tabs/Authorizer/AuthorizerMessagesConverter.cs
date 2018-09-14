using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;

namespace Orchard.Glimpse.Tabs.Authorizer {
    public class AuthorizerMessagesConverter : SerializationConverter<IEnumerable<AuthorizerMessage>> {
        public override object Convert(IEnumerable<AuthorizerMessage> messages) {
            var root = new TabSection("Permission", "Content Id", "Content Type", "Content Name", "User Is Authorized?", "Message", "Time Taken");
            foreach (var message in messages) {
                root.AddRow()
                    .Column(message.Permission)
                    .Column(message.Content?.Id.ToString())
                    .Column(message.Content?.ContentItem.ContentType)
                    .Column(message.Content?.ContentItem.GetContentName())
                    .Column(message.Result ? "Yes" : "No")
                    .Column(message.Result ? null : message.Message)
                    .Column(message.Duration.ToTimingString());
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}