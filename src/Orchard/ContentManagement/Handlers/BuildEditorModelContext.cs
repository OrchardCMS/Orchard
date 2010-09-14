using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorModelContext : BuildModelContext {
        public BuildEditorModelContext(IContent content, IShape model, IShapeHelperFactory shapeHelperFactory) : base(content, model, shapeHelperFactory) {
        }
    }
}