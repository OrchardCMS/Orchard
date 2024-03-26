using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class MembershipValidationService : IMembershipValidationService {
        
        public MembershipValidationService() {
            ShouldCompareLastLogout = true; // default to true for retrocompatibility
        }

        public bool ShouldCompareLastLogout {
            get; set;
            // The public setter allowes injecting this from Site.MyTenant.Config in an AutoFact component
        }

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
            if (ShouldCompareLastLogout && userPart.LastLogoutUtc.HasValue) {

                if (!userPart.LastLoginUtc.HasValue) {
                    return true;
                }

                return userPart.LastLogoutUtc < userPart.LastLoginUtc;
            }

            return true;
        }
    }
}
