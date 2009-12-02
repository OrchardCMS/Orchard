using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {
    public class UserEditViewModel : AdminViewModel {
        public User User { get; set; }
        public ItemEditorViewModel ItemView { get; set; }


        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return User.Id; }
        }

        [Required]
        public string UserName {
            get { return User.As<User>().Record.UserName; }
            set { User.As<User>().Record.UserName = value; }
        }

        [Required]
        public string Email {
            get { return User.As<User>().Record.Email; }
            set { User.As<User>().Record.Email = value; }
        }

    }
}