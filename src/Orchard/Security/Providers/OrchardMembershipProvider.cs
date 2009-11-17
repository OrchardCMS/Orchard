using System;
using System.Web.Security;
using Orchard.Environment;

namespace Orchard.Security.Providers {
    public class OrchardMembershipProvider : MembershipProvider {

        static IMembershipService GetService() {
            return ServiceLocator.Resolve<IMembershipService>();
        }

        static MembershipSettings GetSettings() {
            return GetService().GetSettings();
        }

        private MembershipUser BuildMembershipUser(IUser user) {
            return new MembershipUser(Name,
                user.UserName,
                user.Id,
                user.Email,
                null,
                null,
                true,
                false,
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
            var user = GetService().CreateUser(new CreateUserParams(username, password, email, passwordQuestion, passwordAnswer, isApproved));

            if (user == null) {
                status = MembershipCreateStatus.ProviderError;
                return null;
            }

            status = MembershipCreateStatus.Success;
            return BuildMembershipUser(user);
        }


        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer) {
            throw new NotImplementedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword) {
            var service = GetService();
            var user = service.ValidateUser(username, oldPassword);
            if (user == null)
                return false;

            service.SetPassword(user, newPassword);
            return true;
        }

        public override string ResetPassword(string username, string answer) {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user) {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password) {
            return (GetService().ValidateUser(username, password) != null);
        }

        public override bool UnlockUser(string userName) {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline) {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email) {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData) {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline() {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotImplementedException();
        }

        public override string ApplicationName {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval { get { return GetSettings().EnablePasswordRetrieval; } }
        public override bool EnablePasswordReset { get { return GetSettings().EnablePasswordReset; } }
        public override bool RequiresQuestionAndAnswer { get { return GetSettings().RequiresQuestionAndAnswer; } }
        public override int MaxInvalidPasswordAttempts { get { return GetSettings().MaxInvalidPasswordAttempts; } }
        public override int PasswordAttemptWindow { get { return GetSettings().PasswordAttemptWindow; } }
        public override bool RequiresUniqueEmail { get { return GetSettings().RequiresUniqueEmail; } }
        public override MembershipPasswordFormat PasswordFormat { get { return GetSettings().PasswordFormat; } }
        public override int MinRequiredPasswordLength { get { return GetSettings().MinRequiredPasswordLength; } }
        public override int MinRequiredNonAlphanumericCharacters { get { return GetSettings().MinRequiredNonAlphanumericCharacters; } }
        public override string PasswordStrengthRegularExpression { get { return GetSettings().PasswordStrengthRegularExpression; } }
    }
}
