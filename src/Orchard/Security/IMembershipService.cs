using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        void ReadSettings(MembershipSettings settings);

        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);
    }

    public class MembershipSettings {
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
