using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Orchard.Glimpse.Tabs.Shapes {
    public class ShapesTab : TabBase, ITabSetup, IKey, ILayoutControl {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<ShapeMessage>().ToList();

            if (!messages.Any()) {
                return "There have been no Shape events recorded. If you think there should have been, check that the 'Glimpse for Orchard Shapes' feature is enabled.";
            }

            return messages;
        }

        public override string Name => "Shapes";

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<ShapeMessage>();
        }

        public string Key => "glimpse_orchard_shapes";

        public bool KeysHeadings => false;
    }
}