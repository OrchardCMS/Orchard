using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Users.Events {
    public class LoginUserEventHandler : IUserEventHandler {
        private readonly IClock _clock;
        private readonly IPasswordHistoryService _passwordHistoryService;
        private PasswordHistoryEntry _freezedPasswordEntry;

        public LoginUserEventHandler(IClock clock, IPasswordHistoryService passwordHistoryService) {
            _clock = clock;
            _passwordHistoryService = passwordHistoryService;
        }

        public void Creating(UserContext context) { }

        public void Created(UserContext context) { }

        public void LoggedIn(IUser user) {
            user.As<UserPart>().LastLoginUtc = _clock.UtcNow;
        }

        public void LoggedOut(IUser user) {
            user.As<UserPart>().LastLogoutUtc = _clock.UtcNow;
        }

        public void AccessDenied(IUser user) { }

        public void ChangingPassword(IUser user, string password) {
            var userPart = user.As<UserPart>();
            _freezedPasswordEntry = new PasswordHistoryEntry {
                User = user,
                Password = userPart.Password,
                PasswordSalt = userPart.PasswordSalt,
                HashAlgorithm = userPart.HashAlgorithm,
                PasswordFormat = userPart.PasswordFormat,
                LastPasswordChangeUtc = userPart.LastPasswordChangeUtc
            };
        }

        public void ChangedPassword(IUser user, string password) {
            // If password has changed set to false the Force Password Change flag
            if (user.As<UserPart>().ForcePasswordChange)
                user.As<UserPart>().ForcePasswordChange = false;

            // Store in the password history the previous password
            _passwordHistoryService.CreateEntry(_freezedPasswordEntry);
        }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user) { }

        public void Moderate(IUser user) { 
            user.As<UserPart>().LastLogoutUtc = _clock.UtcNow;
        }

        public void LoggingIn(string userNameOrEmail, string password) { }

        public void LogInFailed(string userNameOrEmail, string password) { }
    }
}
