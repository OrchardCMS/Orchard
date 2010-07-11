using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class RemovePartViewModel : BaseViewModel {
        public string Name { get; set; }
        public EditTypeViewModel Type { get; set; }
    }
}
