using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class EditItemViewModel : BaseViewModel {
        public int Id { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
     public class DisplayItemViewModel : BaseViewModel {
        public ContentItemViewModel Content { get; set; }
    }
}