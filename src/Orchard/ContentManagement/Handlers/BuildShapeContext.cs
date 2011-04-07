using System;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace Orchard.ContentManagement.Handlers {
    public class BuildShapeContext {
        protected BuildShapeContext(IShape shape, IContent content, string groupId, IShapeFactory shapeFactory) {
            Shape = shape;
            ContentItem = content.ContentItem;
            New = shapeFactory;
            GroupId = groupId;
            FindPlacement = (partType, differentiator, defaultLocation) => new PlacementInfo {Location = defaultLocation, Source = String.Empty};
        }

        public dynamic Shape { get; private set; }
        public ContentItem ContentItem { get; private set; }
        public dynamic New { get; private set; }
        public string GroupId { get; private set; }

        public Func<string, string, string, PlacementInfo> FindPlacement { get; set; }
    }
}