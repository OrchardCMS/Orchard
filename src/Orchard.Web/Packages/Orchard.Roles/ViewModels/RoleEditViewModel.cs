using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Roles.ViewModels {
    public class RoleEditViewModel : AdminViewModel {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
