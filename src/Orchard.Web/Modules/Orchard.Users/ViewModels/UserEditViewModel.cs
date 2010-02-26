using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {
    public class UserEditViewModel : BaseViewModel {
        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return User.Item.Id; }
        }

        [Required]
        public string UserName {
            get { return User.Item.Record.UserName; }
            set { User.Item.Record.UserName = value; }
        }

        [Required]
        public string Email {
            get { return User.Item.Record.Email; }
            set { User.Item.Record.Email = value; }
        }

        public ContentItemViewModel<User> User { get; set; }
    }
}