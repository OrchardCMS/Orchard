using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(ItemEditorModel editorModel, IUpdateModel updater)
            : base(editorModel) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}