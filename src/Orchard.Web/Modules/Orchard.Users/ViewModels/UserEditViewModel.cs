using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {
    public class UserEditViewModel  {
        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return User.ContentItem.Id; }
        }

        [Required]
        public string UserName {
            get { return User.As<UserPart>().Record.UserName; }
            set { User.As<UserPart>().Record.UserName = value; }
        }

        [Required]
        public string Email {
            get { return User.As<UserPart>().Record.Email; }
            set { User.As<UserPart>().Record.Email = value; }
        }

        public IContent User { get; set; }
    }
}