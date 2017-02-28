using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using Orchard.Glimpse.Extensions;

namespace Orchard.Glimpse.Tabs.Widgets {
    public class WidgetMessagesConverter : SerializationConverter<IEnumerable<WidgetMessage>> {
        public override object Convert(IEnumerable<WidgetMessage> messages) {
            var root = new TabSection("Id", "Widget Title", "Widget Type", "Layer", "Layer Rule", "Zone", "Position", "Technical Name", "Actions", "Build Display Duration");
            foreach (var message in messages.OrderByDescending(m => m.Duration)) {
                root.AddRow()
                    .Column(message.ContentId)
                    .Column(message.Title)
                    .Column(message.Type)
                    .Column(message.Layer.Name)
                    .Column(message.Layer.LayerRule)
                    .Column(message.Zone)
                    .Column(message.Position)
                    .Column(message.TechnicalName)
                    .Column(@"<a href='" + message.EditUrl + "'>Edit</a>").Raw()
                    .Column(message.Duration.ToTimingString());
            }

            root.AddTimingSummary(messages);

            return root.Build();
        }
    }
}