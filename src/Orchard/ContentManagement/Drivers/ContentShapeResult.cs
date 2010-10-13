using System;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.ContentManagement.Drivers {
    public class ContentShapeResult : DriverResult {
        private string _defaultLocation;
        private readonly string _shapeType;
        private readonly string _prefix;
        private readonly Func<dynamic> _shapeBuilder;

        public ContentShapeResult(string shapeType, string prefix, Func<dynamic> shapeBuilder) {
            _shapeType = shapeType;
            _prefix = prefix;
            _shapeBuilder = shapeBuilder;
        }

        public override void Apply(BuildDisplayContext context) {
            ApplyImplementation(context, context.DisplayType);
        }

        public override void Apply(BuildEditorContext context) {
            ApplyImplementation(context, null);
        }

        private void ApplyImplementation(BuildShapeContext context, string displayType) {
            var location = context.FindPlacement(_shapeType, _defaultLocation);
            if (string.IsNullOrEmpty(location) || location == "-")
                return;

            dynamic parentShape = context.Shape;
            IShape contentShape = _shapeBuilder();
            contentShape.Metadata.Prefix = _prefix;
            contentShape.Metadata.DisplayType = displayType;

            var delimiterIndex = location.IndexOf(':');
            if (delimiterIndex < 0) {
                parentShape.Zones[location].Add(contentShape);
            }
            else {
                var zoneName = location.Substring(0, delimiterIndex);
                var position = location.Substring(delimiterIndex + 1);
                parentShape.Zones[zoneName].Add(contentShape, position);
            }
        }

        public ContentShapeResult Location(string zone) {
            _defaultLocation = zone;
            return this;
        }

        public ContentShapeResult Location(string zone, string position) {
            _defaultLocation = zone + ":" + position;
            return this;
        }

        public ContentShapeResult Location(ContentLocation location) {
            return location.Position == null
                ? Location(location.Zone)
                : Location(location.Zone, location.Position);
        }
    }
}