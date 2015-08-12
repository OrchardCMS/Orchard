using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;

namespace Orchard.Users.Events {
    public class LoginUserEventHandler : IUserEventHandler {
        private readonly IClock _clock;

        public LoginUserEventHandler(IClock clock) {
            _clock = clock;
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

        public void ChangedPassword(IUser user) { }

        public void SentChallengeEmail(IUser user) { }

        public void ConfirmedEmail(IUser user) { }

        public void Approved(IUser user) { }

        public void LoggingIn(string userNameOrEmail, string password) { }

        public void LogInFailed(string userNameOrEmail, string password) { }
    }
}