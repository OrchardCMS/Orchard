using Orchard.Events;

namespace Orchard.Users.Events {
    public interface IUserEventHandler : IEventHandler {
        /// <summary>
        /// Called before a User is created
        /// </summary>
        void Creating(UserContext context);

        /// <summary>
        /// Called once a user has been created
        /// </summary>
        void Created(UserContext context);
    }
}

