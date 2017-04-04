using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using System.Linq;

namespace Orchard.Glimpse.Tabs.Widgets {
    public class WidgetTab : TabBase, ITabSetup, IKey {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<WidgetMessage>().ToList();

            if (!messages.Any()) {
                return null;
            }

            return messages;
        }

        public override string Name { get { return "Widgets"; } }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<WidgetMessage>();
        }

        public string Key { get { return "glimpse_orchard_widgets"; } }
    }
}