using Orchard.DisplayManagement.Implementation;
using Orchard.UI.Zones;

namespace Orchard.Core.Contents {
    public class Shapes : IShapeFactoryEvents {

        public void Creating(ShapeCreatingContext creating) {
            if (creating.ShapeType.StartsWith("Items_Content"))
                creating.Behaviors.Add(new ZoneHoldingBehavior(creating.ShapeFactory));
        }

        public void Created(ShapeCreatedContext created) {
        }
    }
}
