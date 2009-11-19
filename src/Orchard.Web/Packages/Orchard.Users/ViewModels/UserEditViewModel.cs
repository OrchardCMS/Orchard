using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Models;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Models;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {
    public class UserEditViewModel : AdminViewModel {
        public UserModel User { get; set; }
        public IEnumerable<ModelTemplate> Editors { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return User.Id; }
        }

        [Required]
        public string UserName {
            get { return User.As<UserModel>().Record.UserName; }
            set { User.As<UserModel>().Record.UserName = value; }
        }

        [Required]
        public string Email {
            get { return User.As<UserModel>().Record.Email; }
            set { User.As<UserModel>().Record.Email = value; }
        }
    }
}