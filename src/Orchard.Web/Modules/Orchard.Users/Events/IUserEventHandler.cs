using Orchard.Events;
using Orchard.Security;

namespace Orchard.Users.Events {
    public interface IUserEventHandler : IEventHandler {
        /// <summary>
        /// Called before a User is created
        /// </summary>
        void Creating(UserContext context);

        /// <summary>
        /// Called after a user has been created
        /// </summary>
        void Created(UserContext context);

        /// <summary>
        /// Called after a user has logged in
        /// </summary>
        void LoggedIn(IUser user);

        /// <summary>
        /// Called when a user explicitly logs out (as opposed to one whose session cookie simply expires)
        /// </summary>
        void LoggedOut(IUser user);

        /// <summary>
        /// Called when access is denied to a user
        /// </summary>
        void AccessDenied(IUser user);

        /// <summary>
        /// Called after a user has changed password
        /// </summary>
        void ChangedPassword(IUser user);

        /// <summary>
        /// Called after a user has confirmed their email address
        /// </summary>
        void SentChallengeEmail(IUser user);

        /// <summary>
        /// Called after a user has confirmed their email address
        /// </summary>
        void ConfirmedEmail(IUser user);

        /// <summary>
        /// Called after a user has been approved
        /// </summary>
        void Approved(IUser user);
    }
}

