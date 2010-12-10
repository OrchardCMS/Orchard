using System;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.ContentManagement.Drivers {
    public class ContentShapeResult : DriverResult {
        private string _defaultLocation;
        private string _differentiator;
        private readonly string _shapeType;
        private readonly string _prefix;
        private readonly Func<BuildShapeContext, dynamic> _shapeBuilder;

        public ContentShapeResult(string shapeType, string prefix, Func<BuildShapeContext, dynamic> shapeBuilder) {
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
            var location = context.FindPlacement(_shapeType, _differentiator, _defaultLocation);
            if (string.IsNullOrEmpty(location) || location == "-")
                return;

            dynamic parentShape = context.Shape;
            var newShape = _shapeBuilder(context);
            ShapeMetadata newShapeMetadata = newShape.Metadata;
            newShapeMetadata.Prefix = _prefix;
            newShapeMetadata.DisplayType = displayType;

            var delimiterIndex = location.IndexOf(':');
            if (delimiterIndex < 0) {
                parentShape.Zones[location].Add(newShape);
            }
            else {
                var zoneName = location.Substring(0, delimiterIndex);
                var position = location.Substring(delimiterIndex + 1);
                parentShape.Zones[zoneName].Add(newShape, position);
            }
        }

        public ContentShapeResult Location(string zone) {
            _defaultLocation = zone;
            return this;
        }
        public ContentShapeResult Differentiator(string differentiator) {
            _differentiator = differentiator;
            return this;
        }
    }
}