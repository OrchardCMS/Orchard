using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;

namespace Orchard.Glimpse.Tabs.ContentManager {
    public class ContentManagerGetMessagesConverter : SerializationConverter<IEnumerable<ContentManagerGetMessage>> {
        public override object Convert(IEnumerable<ContentManagerGetMessage> messages) {
            var root = new TabSection("Content Id", "Content Type", "Name", "Version Options", "Duration");
            foreach (var message in messages.OrderByDescending(m => m.Duration)) {
                root.AddRow()
                    .Column(message.ContentId)
                    .Column(message.ContentType)
                    .Column(message.Name)
                    .Column(message.VersionOptions)
                    .Column(message.Duration.ToTimingString());
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}