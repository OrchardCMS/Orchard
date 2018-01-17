using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class AccountValidationService : IAccountValidationService {

        private readonly IUserService _userService;

        public AccountValidationService(
            IUserService userService) {

            _userService = userService;
        }

        public Localizer T { get; set; }

        public bool ValidateEmail(string email) {
            IDictionary<string, LocalizedString> validationErrors;
            return ValidateEmail(email, out validationErrors);
        }

        public bool ValidateEmail(string email, out IDictionary<string, LocalizedString> validationErrors) {
            validationErrors = new Dictionary<string, LocalizedString>();
            if (String.IsNullOrEmpty(email)) {
                validationErrors.Add("email", T("You must specify an email address."));
                return false;
            }
            if (email.Length >= UserPart.MaxEmailLength) {
                validationErrors.Add("email", T("The email address you provided is too long."));
                return false;
            }
            if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                validationErrors.Add("email", T("You must specify a valid email address."));
                return false;
            }

            return true;
        }

        public bool ValidatePassword(string password) {
            IDictionary<string, LocalizedString> validationErrors;
            return ValidatePassword(password, out validationErrors);
        }

        public bool ValidatePassword(string password, out IDictionary<string, LocalizedString> validationErrors) {
            return _userService.PasswordMeetsPolicies(password, out validationErrors);
        }

        public bool ValidateUserName(string userName) {
            IDictionary<string, LocalizedString> validationErrors;
            return ValidateUserName(userName, out validationErrors);
        }

        public bool ValidateUserName(string userName, out IDictionary<string, LocalizedString> validationErrors) {
            validationErrors = new Dictionary<string, LocalizedString>();

            if (String.IsNullOrEmpty(userName)) {
                validationErrors.Add("username", T("You must specify a username."));
                return false;
            }
            if (userName.Length >= UserPart.MaxUserNameLength) {
                validationErrors.Add("username", T("The username you provided is too long."));
                return false;
            }

            return true;
        }
    }
}