using Orchard.Security;
using System;
namespace Orchard.Users.Services {
    public interface IUserService : IDependency {
        string VerifyUserUnicity(string userName, string email);
        string VerifyUserUnicity(int id, string userName, string email);

        void SendChallengeEmail(IUser user, string url);
        IUser ValidateChallenge(string challengeToken);

        bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl);
        IUser ValidateLostPassword(string nonce);

        string GetNonce(IUser user);
    }
}