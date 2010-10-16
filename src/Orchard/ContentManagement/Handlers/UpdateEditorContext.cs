using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorContext : BuildEditorContext {
        public UpdateEditorContext(IShape model, IContent content, IUpdateModel updater, IShapeFactory shapeFactory)
            : base(model, content, shapeFactory) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}