using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class CreateItemViewModel : BaseViewModel {
        public string Id { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
}
