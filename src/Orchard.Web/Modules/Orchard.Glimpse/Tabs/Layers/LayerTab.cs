using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Orchard.Glimpse.Tabs.Layers {
    public class LayerTab : TabBase, ITabSetup, IKey {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<LayerMessage>().ToList();

            if (!messages.Any()) {
                return "There have been no Layer events recorded. If you think there should have been, check that the 'Glimpse for Orchard Layers' feature is enabled.";
            }

            return messages;
        }

        public override string Name {
            get { return "Layers"; }
        }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<LayerMessage>();
        }

        public string Key {
            get { return "glimpse_orchard_layers"; }
        }
    }
}