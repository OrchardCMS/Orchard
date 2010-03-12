using System.Web.Security;

namespace Orchard.Security {
    public class MembershipSettings {
        public MembershipSettings() {
            EnablePasswordRetrieval = false;
            EnablePasswordReset = true;
            RequiresQuestionAndAnswer = true;
            RequiresUniqueEmail = true;
            MaxInvalidPasswordAttempts = 5;
            PasswordAttemptWindow = 10;
            MinRequiredPasswordLength = 7;
            MinRequiredNonAlphanumericCharacters = 1;
            PasswordStrengthRegularExpression = "";
            PasswordFormat = MembershipPasswordFormat.Hashed;
        }

        public bool EnablePasswordRetrieval { get; set; }
        public bool EnablePasswordReset { get; set; }
        public bool RequiresQuestionAndAnswer { get; set; }
        public int MaxInvalidPasswordAttempts { get; set; }
        public int PasswordAttemptWindow { get; set; }
        public bool RequiresUniqueEmail { get; set; }
        public MembershipPasswordFormat PasswordFormat { get; set; }
        public int MinRequiredPasswordLength { get; set; }
        public int MinRequiredNonAlphanumericCharacters { get; set; }
        public string PasswordStrengthRegularExpression { get; set; }
    }
}