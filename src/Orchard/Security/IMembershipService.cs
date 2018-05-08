namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        IMembershipSettings GetSettings();

        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);
        IUser ValidateUser(string userNameOrEmail, string password);
        void SetPassword(IUser user, string password);

        bool PasswordIsExpired(IUser user, int days);
    }
}
