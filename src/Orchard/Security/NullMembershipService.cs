using System;

namespace Orchard.Security {
    /// <summary>
    /// Provides a default implementation of <c>IMembershipService</c> used only for dependency resolution
    /// in a setup context. No members on this implementation will ever be called; at the time when this
    /// interface is actually used in a tenant, another implementation is assumed to have suppressed it.
    /// </summary>
    public class NullMembershipService : IMembershipService {
        public IUser CreateUser(CreateUserParams createUserParams) {
            throw new NotImplementedException();
        }

        public MembershipSettings GetSettings() {
            throw new NotImplementedException();
        }

        public IUser GetUser(string username) {
            throw new NotImplementedException();
        }

        public void SetPassword(IUser user, string password) {
            throw new NotImplementedException();
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            throw new NotImplementedException();
        }
    }
}
