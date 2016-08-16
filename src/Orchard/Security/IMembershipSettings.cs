using System.Web.Security;

namespace Orchard.Security {
    public interface IMembershipSettings {
        bool UsersCanRegister { get; set; }
        bool UsersMustValidateEmail { get; set; }
        string ValidateEmailRegisteredWebsite { get; set; }
        string ValidateEmailContactEMail { get; set; }
        bool UsersAreModerated { get; set; }
        bool NotifyModeration { get; set; }
        string NotificationsRecipients { get; set; }
        bool EnableLostPassword { get; set; }
        bool EnableCustomPasswordPolicy { get; set; }
        int MinimumPasswordLength { get; set; }
        bool EnablePasswordUppercaseRequirement { get; set; }
        bool EnablePasswordLowercaseRequirement { get; set; }
        bool EnablePasswordNumberRequirement { get; set; }
        bool EnablePasswordSpecialRequirement { get; set; }
        bool EnablePasswordExpiration { get; set; }
        int PasswordExpirationTimeInDays { get; set; }
        MembershipPasswordFormat PasswordFormat { get; set; }
    }
}