using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class EditItemViewModel  {
        public int Id { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
     public class DisplayItemViewModel {
        public ContentItemViewModel Content { get; set; }
    }
}