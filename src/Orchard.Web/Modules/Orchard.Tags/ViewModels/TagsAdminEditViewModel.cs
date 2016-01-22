using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminEditViewModel {
        public int Id { get; set; }
        [Required, DisplayName("Name")]
        public string TagName { get; set; } 
    }
}
