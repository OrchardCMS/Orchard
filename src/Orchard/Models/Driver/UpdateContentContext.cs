using Orchard.Models.ViewModels;

namespace Orchard.Models.Driver {
    public class UpdateContentContext : GetEditorsContext {
        public UpdateContentContext(ItemEditorViewModel itemView, string groupName, IUpdateModel updater)
            : base(itemView, groupName) {
            Updater = updater;
        }

        public IUpdateModel Updater { get; set; }
    }
}