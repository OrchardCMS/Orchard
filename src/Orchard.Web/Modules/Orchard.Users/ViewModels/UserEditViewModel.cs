using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {
    public class UserEditViewModel  {
        [Required]
        public string UserName {
            get { return User.As<UserPart>().Record.UserName; }
            set { User.As<UserPart>().Record.UserName = value; }
        }

        [Required]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*\\.([a-z]{2,4})$")]
        public string Email {
            get { return User.As<UserPart>().Record.Email; }
            set { User.As<UserPart>().Record.Email = value; }
        }

        public IContent User { get; set; }
    }
}