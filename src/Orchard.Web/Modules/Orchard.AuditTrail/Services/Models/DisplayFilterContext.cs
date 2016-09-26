using Orchard.DisplayManagement;

namespace Orchard.AuditTrail.Services.Models {
    public class DisplayFilterContext {
        public DisplayFilterContext(IShapeFactory shapeFactory, Filters filters, dynamic filterDisplay) {
            ShapeFactory = shapeFactory;
            Filters = filters;
            FilterDisplay = filterDisplay;
        }

        public dynamic ShapeFactory { get; set; }
        public Filters Filters { get; set; }
        public dynamic FilterDisplay { get; set; }
    }
}