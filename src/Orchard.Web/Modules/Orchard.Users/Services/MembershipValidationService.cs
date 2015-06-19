using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MembershipValidationService : IMembershipValidationService {

        public bool CanAuthenticateWithCookie(IUser user) {
            var userPart = user as UserPart;

            if (userPart == null) {
                return false;
            }

            // user has not been approved or is currently disabled
            if (userPart.RegistrationStatus != UserStatus.Approved) {
                return false;
            }

            // if the user has logged out, a cookie should not be accepted
            if (userPart.LastLogoutUtc.HasValue) {

                if (!userPart.LastLoginUtc.HasValue) {
                    return true;
                }

                return userPart.LastLogoutUtc < userPart.LastLoginUtc;
            }

            return true;
        }
    }
}