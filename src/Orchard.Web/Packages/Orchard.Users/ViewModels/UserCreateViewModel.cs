using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;

namespace Orchard.Users.ViewModels {
    public class UserCreateViewModel : AdminViewModel {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }
    }
}