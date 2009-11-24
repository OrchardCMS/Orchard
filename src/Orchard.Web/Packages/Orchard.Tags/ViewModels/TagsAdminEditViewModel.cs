using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminEditViewModel : AdminViewModel {
        public int Id { get; set; }
        public string TagName { get; set; } 
    }
}
