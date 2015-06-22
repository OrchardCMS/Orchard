using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels
{
    public class UserIndexViewModel
    {
        public int Id { get; set; }
        public UserStatus RegistrationStatus { get; set; }
        public UserStatus EmailStatus { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }

        public bool IsChecked { get; set; }

        public UserIndexViewModel()
        {
            this.Id = -1;
            this.UserName = string.Empty;
            this.Email = string.Empty;

            this.IsChecked = false;

            this.RegistrationStatus = UserStatus.Pending;
            this.EmailStatus = UserStatus.Pending;
        }

        public UserIndexViewModel(UserPart userPart)
        {
            MapPart(userPart);
        }

        internal void MapPart(UserPart part)
        {
            this.Id = part.Id;
            this.RegistrationStatus = part.RegistrationStatus;
            this.EmailStatus = part.EmailStatus;

            this.UserName = part.UserName;
            this.Email = part.Email;
        }
    }
}