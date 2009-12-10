using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(ItemEditorModel editorModel, string groupName, IUpdateModel updater)
            : base(editorModel, groupName) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; set; }
    }
}