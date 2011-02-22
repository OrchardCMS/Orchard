using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorContext : BuildEditorContext {
        public UpdateEditorContext(IShape model, IContent content, IUpdateModel updater, string groupInfoId, IShapeFactory shapeFactory)
            : base(model, content, groupInfoId, shapeFactory) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}