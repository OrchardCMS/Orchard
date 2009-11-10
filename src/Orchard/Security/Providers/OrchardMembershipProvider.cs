using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Orchard.Environment;

namespace Orchard.Security.Providers {
    public class OrchardMembershipProvider : MembershipProvider {
        public OrchardMembershipProvider() {
            int x =5;
        }

        static IMembershipService GetService() {
            return ServiceLocator.Resolve<IMembershipService>();
        }

        static MembershipSettings GetSettings() {
            var settings = new MembershipSettings {
                EnablePasswordRetrieval = false,
                EnablePasswordReset = true,
                RequiresQuestionAndAnswer = true,
                RequiresUniqueEmail = true,
                MaxInvalidPasswordAttempts = 5,
                PasswordAttemptWindow = 10,
                MinRequiredPasswordLength = 7,
                MinRequiredNonAlphanumericCharacters = 1,
                PasswordStrengthRegularExpression = "",
                PasswordFormat = MembershipPasswordFormat.Hashed,
            };
            GetService().ReadSettings(settings);
            return settings;
        }


        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer) {
            throw new NotImplementedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword) {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer) {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user) {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password) {
            throw new NotImplementedException();
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
