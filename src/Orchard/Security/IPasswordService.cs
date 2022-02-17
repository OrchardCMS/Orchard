namespace Orchard.Security {
    public interface IPasswordService : IDependency {
        bool IsMatch(PasswordContext context, string plaintextPassword);
    }
}