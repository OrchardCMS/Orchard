using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(ItemEditorModel editorModel, string groupName, IUpdateModel updater, string templatePath)
            : base(editorModel, groupName, templatePath) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}