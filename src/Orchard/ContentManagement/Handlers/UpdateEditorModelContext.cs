using Orchard.Mvc.ViewModels;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorModelContext : BuildEditorModelContext {
        public UpdateEditorModelContext(ContentItemViewModel viewModel, IUpdateModel updater)
            : base(viewModel) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; private set; }
    }
}