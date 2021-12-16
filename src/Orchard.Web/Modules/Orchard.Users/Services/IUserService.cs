using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;

namespace Orchard.Users.Services {
    public interface IUserService : IDependency {
        bool VerifyUserUnicity(string userName, string email);
        bool VerifyUserUnicity(int id, string userName, string email);
        UserPart GetUserByNameOrEmail(string usernameOrEmail);

        void SendChallengeEmail(IUser user, Func<string, string> createUrl);
        IUser ValidateChallenge(string challengeToken);

        bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl);
        IUser ValidateLostPassword(string nonce);

        string CreateNonce(IUser user, TimeSpan delay);
        bool DecryptNonce(string challengeToken, out string username, out DateTime validateByUtc);

        bool PasswordMeetsPolicies(string password, IUser user, out IDictionary<string, LocalizedString> validationErrors);
    }
}