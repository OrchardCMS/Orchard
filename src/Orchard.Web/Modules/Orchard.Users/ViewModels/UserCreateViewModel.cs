using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Users.ViewModels {
    public class UserCreateViewModel  {
        [Required]
        public string UserName { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*\\.([a-z]{2,4})$")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public IContent User { get; set; }
    }
}