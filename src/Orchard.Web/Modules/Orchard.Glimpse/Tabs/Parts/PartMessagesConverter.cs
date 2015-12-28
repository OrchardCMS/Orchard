using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;
using Orchard.Utility.Extensions;

namespace Orchard.Glimpse.Tabs.Parts {
    public class PartMessagesConverter : SerializationConverter<IEnumerable<PartMessage>> {
        public override object Convert(IEnumerable<PartMessage> messages) {
            var root = new TabSection("Content Id", "Content Name", "Content Type", "Part", "Display Type", "Duration");
            foreach (var message in messages) {
                root.AddRow()
                    .Column(message.ContentId)
                    .Column(message.ContentName)
                    .Column(message.ContentType.CamelFriendly())
                    .Column(message.PartDefinition?.Name.CamelFriendly())
                    .Column(message.DisplayType)
                    .Column(message.Duration.ToTimingString());
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}