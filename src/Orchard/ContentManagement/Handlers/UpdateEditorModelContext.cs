using Orchard.DisplayManagement;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(IContent content, IUpdateModel updater, IShape model, IShapeHelperFactory shapeHelperFactory)
            : base(content, model, shapeHelperFactory) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}