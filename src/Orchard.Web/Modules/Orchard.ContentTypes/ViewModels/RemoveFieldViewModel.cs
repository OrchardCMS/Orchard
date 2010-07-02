using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class RemoveFieldViewModel : BaseViewModel {
        public string Name { get; set; }
        public EditPartViewModel Part { get; set; }
    }
}
