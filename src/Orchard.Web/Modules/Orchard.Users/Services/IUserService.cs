namespace Orchard.Users.Services {
    public interface IUserService : IDependency {
        string VerifyUserUnicity(string userName, string email);
        string VerifyUserUnicity(int id, string userName, string email);
    }
}