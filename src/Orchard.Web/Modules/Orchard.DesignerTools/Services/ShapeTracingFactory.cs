using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;

namespace Orchard.DesignerTools.Services {
    [OrchardFeature("Orchard.DesignerTools")]
    public class ShapeTracingFactory : IShapeFactoryEvents {
        public void Creating(ShapeCreatingContext context) {
        }

        public void Created(ShapeCreatedContext context) {
            if (context.ShapeType != "Layout" && context.ShapeType != "DocumentZone") {
                context.Shape.Metadata.Wrappers.Add("ShapeTracing_Wrapper");
            }
        }
    }
}