using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Drivers {
    public class ContentShapeResult : DriverResult {
        public dynamic Shape { get; set; }
        public string Prefix { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }

        public ContentShapeResult(dynamic shape, string prefix) {
            Shape = shape;
            Prefix = prefix;
        }

        public override void Apply(BuildDisplayContext context) {
            context.Shape.Zones[Zone].Add(Shape, Position);
        }

        public override void Apply(BuildEditorContext context) {
            IShape iShape = Shape;
            if (iShape != null )
                Shape.Metadata.Prefix = Prefix;
            context.Shape.Zones[Zone].Add(Shape, Position);
        }

        public ContentShapeResult Location(string zone) {
            Zone = zone;
            return this;
        }

        public ContentShapeResult Location(string zone, string position) {
            Zone = zone;
            Position = position;
            return this;
        }

        public ContentShapeResult Location(ContentLocation location) {
            return location.Position == null
                ? Location(location.Zone)
                : Location(location.Zone, location.Position);
        }
    }
}