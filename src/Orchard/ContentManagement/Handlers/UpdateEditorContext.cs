using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorContext : BuildEditorContext {
        public UpdateEditorContext(IShape model, IContent content, IUpdateModel updater, IShapeHelperFactory shapeHelperFactory)
            : base(model, content, shapeHelperFactory) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}