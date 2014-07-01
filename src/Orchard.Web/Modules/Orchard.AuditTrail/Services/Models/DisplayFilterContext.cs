using Orchard.DisplayManagement;

namespace Orchard.AuditTrail.Services.Models {
    public class DisplayFilterContext {
        public DisplayFilterContext(IShapeFactory shapeFactory, Filters filters, dynamic filterLayout) {
            ShapeFactory = shapeFactory;
            Filters = filters;
            FilterLayout = filterLayout;
        }

        public dynamic ShapeFactory { get; set; }
        public Filters Filters { get; set; }
        public dynamic FilterLayout { get; set; }
    }
}