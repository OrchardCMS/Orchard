using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorContext : BuildShapeContext {
        public BuildEditorContext(IShape model, IContent content, IShapeHelperFactory shapeHelperFactory)
            : base(model, content, shapeHelperFactory) {
        }
    }
}