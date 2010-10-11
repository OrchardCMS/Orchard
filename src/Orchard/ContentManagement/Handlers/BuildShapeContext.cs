using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildShapeContext {
        protected BuildShapeContext(IShape shape, IContent content, IShapeHelperFactory shapeHelperFactory) {
            Shape = shape;
            ContentItem = content.ContentItem;
            New = shapeHelperFactory.CreateHelper();
        }

        public dynamic Shape { get; private set; }
        public ContentItem ContentItem { get; private set; }
        public dynamic New { get; private set; }
    }
}