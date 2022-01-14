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

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool ValidatePassword(AccountValidationContext context) {
            IDictionary<string, LocalizedString> validationErrors;
            _userService.PasswordMeetsPolicies(context.Password, context.User, out validationErrors);
            if (validationErrors != null && validationErrors.Any()) {
                foreach (var err in validationErrors) {
                    if (!context.ValidationErrors.ContainsKey(err.Key)) {
                        context.ValidationErrors.Add(err);
                    }
                }
            }

            return context.ValidationSuccessful;
        }

        public bool ValidateUserName(AccountValidationContext context) {

            if (string.IsNullOrWhiteSpace(context.UserName)) {
                context.ValidationErrors.Add("username", T("You must specify a username."));
            } else if (context.UserName.Length >= UserPart.MaxUserNameLength) {
                context.ValidationErrors.Add("username", T("The username you provided is too long."));
            }

            return context.ValidationSuccessful;
        }

        public bool ValidateEmail(AccountValidationContext context) {

            if (string.IsNullOrWhiteSpace(context.Email)) {
                context.ValidationErrors.Add("email", T("You must specify an email address."));
            } else if (context.Email.Length >= UserPart.MaxEmailLength) {
                context.ValidationErrors.Add("email", T("The email address you provided is too long."));
            } else if (!Regex.IsMatch(context.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                context.ValidationErrors.Add("email", T("You must specify a valid email address."));
            }

            return context.ValidationSuccessful;
        }
        
    }
}