using System.Collections.Generic;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;

namespace Orchard.Glimpse.Tabs.Cache {
    public class CacheMessagesConverter : SerializationConverter<IEnumerable<CacheMessage>> {
        public override object Convert(IEnumerable<CacheMessage> messages) {
            var root = new TabSection("Action", "Valid For", "Key", "Result", "Value", "Time Taken");
            foreach (var message in messages) {
                root.AddRow()
                    .Column(message.Action)
                    .Column(message.ValidFor?.ToReadableString())
                    .Column(message.Key)
                    .Column(message.Result)
                    .Column(message.Value)
                    .Column(message.Duration.ToTimingString())
                    .QuietIf(message.Result == "Miss");
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}