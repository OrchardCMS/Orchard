using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorContext : BuildShapeContext {
        public BuildEditorContext(IShape model, IContent content, IShapeFactory shapeFactory)
            : base(model, content, shapeFactory) {
        }
    }
}