using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Dashboards.Shapes {
    public class LayoutPartShape : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Parts_Layout").OnDisplaying(context => {
                if (context.ShapeMetadata.DisplayType != "Dashboard")
                    return;

                context.ShapeMetadata.Alternates.Add("Parts_Layout_Dashboard");
            });
        }
    }
}