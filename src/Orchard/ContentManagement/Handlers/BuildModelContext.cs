using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildModelContext {
        protected BuildModelContext(IContent content, IShape model, IShapeHelperFactory shapeHelperFactory) {
            ContentItem = content.ContentItem;
            Model = model;
            Shape = shapeHelperFactory.CreateHelper();
        }

        public ContentItem ContentItem { get; private set; }
        public dynamic Model { get; private set; }
        public dynamic Shape { get; private set; }
    }
}