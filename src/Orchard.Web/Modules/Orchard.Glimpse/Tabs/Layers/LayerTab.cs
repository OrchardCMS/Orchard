using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Orchard.Glimpse.Tabs.Layers {
    public class LayerTab : TabBase, ITabSetup, IKey {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<LayerMessage>().ToList();

            if (!messages.Any()) {
                return null;
            }

            return messages;
        }

        public override string Name { get { return "Layers"; } }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<LayerMessage>();
        }

        public string Key { get { return "glimpse_orchard_layers"; } }
    }
}