using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Tags.ViewModels {
    public class TagsAdminEditViewModel : AdminViewModel {
        public int Id { get; set; }
        [Required, DisplayName("Name:")]
        public string TagName { get; set; } 
    }
}
