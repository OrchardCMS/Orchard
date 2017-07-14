using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;

namespace Orchard.Glimpse.Tabs.Layers {
    public class LayerMessagesConverter : SerializationConverter<IEnumerable<LayerMessage>> {
        public override object Convert(IEnumerable<LayerMessage> messages) {
            var root = new TabSection("Layer Name", "Layer Rule", "Active", "Actions", "Evaluation Time");
            foreach (var message in messages.OrderByDescending(m => m.Duration)) {
                root.AddRow()
                    .Column(message.Name)
                    .Column(message.Rule)
                    .Column(message.Active ? "Yes" : "No")
                    .Column(@"<a href='" + message.EditUrl + "'>Edit</a>").Raw()
                    .Column(message.Duration.ToTimingString())
                    .QuietIf(!message.Active);
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}