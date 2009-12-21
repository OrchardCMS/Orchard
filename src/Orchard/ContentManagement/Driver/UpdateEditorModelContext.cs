using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(ItemEditorModel editorModel, IUpdateModel updater)
            : base(editorModel) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}