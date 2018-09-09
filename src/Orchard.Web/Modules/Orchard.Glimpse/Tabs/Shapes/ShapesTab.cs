using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Orchard.Glimpse.Tabs.Shapes {
    public class ShapesTab : TabBase, ITabSetup, IKey, ILayoutControl {
        public override object GetData(ITabContext context) {
            var messages = context.GetMessages<ShapeMessage>().ToList();

            if (!messages.Any()) {
                return null;
            }

            return messages;
        }

        public override string Name { get { return "Shapes"; } }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<ShapeMessage>();
        }

        public string Key { get { return "glimpse_orchard_shapes"; } }

        public bool KeysHeadings { get { return false; } }
    }
}