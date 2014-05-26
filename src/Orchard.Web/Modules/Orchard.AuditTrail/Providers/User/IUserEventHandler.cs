using Orchard.Events;
using Orchard.Security;

namespace Orchard.AuditTrail.Providers.User {
    public interface IUserEventHandler : IEventHandler {
        /// <summary>
        /// Called after a user has changed password
        /// </summary>
        void ChangedPassword(IUser user);
    }
}