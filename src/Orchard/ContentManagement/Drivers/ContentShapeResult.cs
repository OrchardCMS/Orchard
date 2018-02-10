using System;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement.Shapes;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;

namespace Orchard.ContentManagement.Drivers {
    public class ContentShapeResult : DriverResult {
        private string _defaultLocation;
        private string _differentiator;
        private readonly string _shapeType;
        private readonly string _prefix;
        private readonly Func<BuildShapeContext, dynamic> _shapeBuilder;
        private string _groupId;

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
            var placement = context.FindPlacement(_shapeType, _differentiator, _defaultLocation);
            if (String.IsNullOrEmpty(placement.Location) || placement.Location == "-")
                return;

            // Parse group placement.
            var group = placement.GetGroup();
            if (!String.IsNullOrEmpty(group)) {
                _groupId = group;
            }

            if (!String.Equals(context.GroupId ?? "", _groupId ?? "", StringComparison.OrdinalIgnoreCase))
                return;

            dynamic parentShape = context.Shape;
            context.ContentPart = ContentPart;

            var newShape = _shapeBuilder(context);

            // Ignore it if the driver returned a null shape.
            if (newShape == null) {
                return;
            }

            // Add a ContentPart property to the final shape.
            if (ContentPart != null && newShape.ContentPart == null) {
                newShape.ContentPart = ContentPart;
            }

            // Add a ContentField property to the final shape.
            if (ContentField != null && newShape.ContentField == null) {
                newShape.ContentField = ContentField;
            }

            ShapeMetadata newShapeMetadata = newShape.Metadata;
            newShapeMetadata.Prefix = _prefix;
            newShapeMetadata.DisplayType = displayType;
            newShapeMetadata.PlacementSource = placement.Source;
            newShapeMetadata.Tab = placement.GetTab();
            
            // If a specific shape is provided, remove all previous alternates and wrappers.
            if (!String.IsNullOrEmpty(placement.ShapeType)) {
                newShapeMetadata.Type = placement.ShapeType;
                newShapeMetadata.Alternates.Clear();
                newShapeMetadata.Wrappers.Clear();
            }

            foreach (var alternate in placement.Alternates) {
                newShapeMetadata.Alternates.Add(alternate);
            }

            foreach (var wrapper in placement.Wrappers) {
                newShapeMetadata.Wrappers.Add(wrapper);
            }

            // Check if the zone name is in reference of Layout, e.g. /AsideSecond.
            if (placement.IsLayoutZone()) {
                parentShape = context.Layout;
            }

            var position = placement.GetPosition();
            var zone = placement.GetZone();

            if (String.IsNullOrEmpty(position)) {
                parentShape.Zones[zone].Add(newShape);
            }
            else {
                parentShape.Zones[zone].Add(newShape, position);
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

        public ContentShapeResult OnGroup(string groupId) {
            _groupId = groupId.ToSafeName();
            return this;
        }

        public string GetDifferentiator() {
            return _differentiator;
        }

        public string GetGroup() {
            return _groupId;
        }

        public string GetLocation() {
            return _defaultLocation;
        }

        public string GetShapeType() {
            return _shapeType;
        }

        public bool WasDisplayed(UpdateEditorContext context) {
            ShapeDescriptor descriptor;
            if (context.ShapeTable.Descriptors.TryGetValue(_shapeType, out descriptor)) {
                var placementContext = new ShapePlacementContext {
                    Content = context.ContentItem,
                    ContentType = context.ContentItem.ContentType,
                    Differentiator = _differentiator,
                    DisplayType = null,
                    Path = context.Path
                };

                var placementInfo = descriptor.Placement(placementContext);

                var location = placementInfo.Location;

                if (String.IsNullOrEmpty(location) || location == "-") {
                    return false;
                }

                var editorGroup = _groupId;
                if (String.IsNullOrEmpty(editorGroup)) {
                    editorGroup = placementInfo.GetGroup() ?? "";
                }

                var contextGroup = context.GroupId ?? "";

                if (!String.Equals(editorGroup, contextGroup, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }

            return true;
        }
    }
}