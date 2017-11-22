using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        IMembershipSettings GetSettings();

        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);
        IUser ValidateUser(string userNameOrEmail, string password, out List<LocalizedString> validationErrors);
        void SetPassword(IUser user, string password);

        bool PasswordIsExpired(IUser user, int days);
    }
}
