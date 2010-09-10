using Orchard.DisplayManagement.Implementation;

namespace Orchard.DevTools {
    public class Shapes : IShapeFactoryEvents {
        public void Creating(ShapeCreatingContext context) {
            
        }

        public void Created(ShapeCreatedContext context) {
            if (context.ShapeType != "Layout")
                context.Shape.Metadata.Wrappers.Add("ThinBorder");
            if (context.ShapeType == "Header")
                context.Shape.Metadata.Wrappers.Add("HackStyle");
        }
    }
}
