namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        MembershipSettings GetSettings();

        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);

        IUser ValidateUser(string userNameOrEmail, string password);
        void SetPassword(IUser user, string password);

    }
}
