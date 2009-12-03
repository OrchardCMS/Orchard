using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class UpdateEditorViewModelContext : GetEditorViewModelContext {
        public UpdateEditorViewModelContext(ItemEditorViewModel viewModel, string groupName, IUpdateModel updater)
            : base(viewModel, groupName) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; set; }
    }
}