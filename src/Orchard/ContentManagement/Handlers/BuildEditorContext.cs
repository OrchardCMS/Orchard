using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorContext : BuildShapeContext {
        public BuildEditorContext(IShape model, IContent content, string groupInfoId, IShapeFactory shapeFactory)
            : base(model, content, shapeFactory) {
            GroupInfoId = groupInfoId;
        }

        public string GroupInfoId { get; private set; }
    }
}