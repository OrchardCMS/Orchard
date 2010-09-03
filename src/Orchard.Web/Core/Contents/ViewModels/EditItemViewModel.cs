namespace Orchard.Core.Contents.ViewModels {
#if REFACTORING
    public class EditItemViewModel  {
        public int Id { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
     public class DisplayItemViewModel {
        public ContentItemViewModel Content { get; set; }
    }
#endif
}