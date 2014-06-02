using Orchard.Events;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.User {
    public interface IUserEventHandler : IEventHandler {

        /// <summary>
        /// Called after a user has logged in.
        /// </summary>
        void LoggedIn(IUser user);

        /// <summary>
        /// Called when a user explicitly logs out (as opposed to one whose session cookie simply expires).
        /// </summary>
        void LoggedOut(IUser user);

        /// <summary>
        /// Called when a login attempt failed.
        /// </summary>
        void LogInFailed(string userNameOrEmail, string password);

        /// <summary>
        /// Called after a user has changed password.
        /// </summary>
        void ChangedPassword(IUser user);
    }
}