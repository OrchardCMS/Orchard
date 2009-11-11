using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Roles.ViewModels {
    public class RoleCreateViewModel : AdminViewModel {
        [Required]
        public string Name { get; set; }
    }
}
