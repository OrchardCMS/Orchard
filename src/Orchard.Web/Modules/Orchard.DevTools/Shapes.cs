using Orchard.DisplayManagement.Implementation;

namespace Orchard.DevTools {
    public class Shapes : IShapeFactoryEvents {
        public void Creating(ShapeCreatingContext context) {
            
        }

        public void Created(ShapeCreatedContext context) {
            context.Shape.Metadata.Wrappers.Add("ThinBorder");
        }
    }
}
