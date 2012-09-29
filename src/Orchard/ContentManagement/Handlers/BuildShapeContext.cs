using System;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Logging;

namespace Orchard.ContentManagement.Handlers {
    public class BuildShapeContext {
        protected BuildShapeContext(IShape shape, IContent content, string groupId, IShapeFactory shapeFactory) {
            Shape = shape;
            Content = content;
            ContentItem = content.ContentItem;
            New = shapeFactory;
            GroupId = groupId;
            FindPlacement = (partType, differentiator, defaultLocation) => new PlacementInfo {Location = defaultLocation, Source = String.Empty};
        }

        public dynamic Shape { get; private set; }
        public IContent Content { get; private set; }
        public ContentItem ContentItem { get; private set; }
        public dynamic New { get; private set; }
        public IShape Layout { get; set; }
        public string GroupId { get; private set; }
        public ContentPart ContentPart { get; set; }
        public ILogger Logger { get; set; }

        public Func<string, string, string, PlacementInfo> FindPlacement { get; set; }
    }
}