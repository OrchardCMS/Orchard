namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        MembershipSettings GetSettings();

        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);
        IUser ValidateUser(string userNameOrEmail, string password);
        void SetPassword(IUser user, string password);

        /// <summary>
        /// Returns <c>true</c> if the user is allowed to login from an auth cookie, <c>false</c> otherwise.
        /// </summary>
        bool CanAuthenticateWithCookie(IUser user);
    }
}
