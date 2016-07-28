using Orchard.ContentManagement;
using Orchard.Security;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace Orchard.Users.Models {
    public class RegistrationSettingsPart : ContentPart, IMembershipSettings {
        public bool UsersCanRegister {
            get { return this.Retrieve(x => x.UsersCanRegister); }
            set { this.Store(x => x.UsersCanRegister, value); }
        }

        public bool UsersMustValidateEmail {
            get { return this.Retrieve(x => x.UsersMustValidateEmail); }
            set { this.Store(x => x.UsersMustValidateEmail, value); }
        }

        public string ValidateEmailRegisteredWebsite {
            get { return this.Retrieve(x => x.ValidateEmailRegisteredWebsite); }
            set { this.Store(x => x.ValidateEmailRegisteredWebsite, value); }
        }
        
        public string ValidateEmailContactEMail {
            get { return this.Retrieve(x => x.ValidateEmailContactEMail); }
            set { this.Store(x => x.ValidateEmailContactEMail, value); }
        }

        public bool UsersAreModerated {
            get { return this.Retrieve(x => x.UsersAreModerated); }
            set { this.Store(x => x.UsersAreModerated, value); }
        }

        public bool NotifyModeration {
            get { return this.Retrieve(x => x.NotifyModeration); }
            set { this.Store(x => x.NotifyModeration, value); }
        }

        public string NotificationsRecipients {
            get { return this.Retrieve(x => x.NotificationsRecipients); }
            set { this.Store(x => x.NotificationsRecipients, value); }
        }

        public bool EnableLostPassword {
            get { return this.Retrieve(x => x.EnableLostPassword); }
            set { this.Store(x => x.EnableLostPassword, value); }
        }

        public bool EnableCustomPasswordPolicy {
            get { return this.Retrieve(x => x.EnableCustomPasswordPolicy); }
            set { this.Store(x => x.EnableCustomPasswordPolicy, value); }
        }

        [Range(1, int.MaxValue, ErrorMessage = "The minimum password length must be at least 1.")]
        public int MinimumPasswordLength {
            get { return this.Retrieve(x => x.MinimumPasswordLength, 7); }
            set { this.Store(x => x.MinimumPasswordLength, value); }
        }

        public bool EnablePasswordUppercaseRequirement {
            get { return this.Retrieve(x => x.EnablePasswordUppercaseRequirement); }
            set { this.Store(x => x.EnablePasswordUppercaseRequirement, value); }
        }

        public bool EnablePasswordLowercaseRequirement {
            get { return this.Retrieve(x => x.EnablePasswordLowercaseRequirement); }
            set { this.Store(x => x.EnablePasswordLowercaseRequirement, value); }
        }

        public bool EnablePasswordNumberRequirement {
            get { return this.Retrieve(x => x.EnablePasswordNumberRequirement); }
            set { this.Store(x => x.EnablePasswordNumberRequirement, value); }
        }

        public bool EnablePasswordSpecialRequirement {
            get { return this.Retrieve(x => x.EnablePasswordSpecialRequirement); }
            set { this.Store(x => x.EnablePasswordSpecialRequirement, value); }
        }

        public bool EnablePasswordExpiration {
            get { return this.Retrieve(x => x.EnablePasswordExpiration); }
            set { this.Store(x => x.EnablePasswordExpiration, value); }
        }

        [Range(1, int.MaxValue, ErrorMessage = "The password expiration time must be a minimum of 1 day.")]
        public int PasswordExpirationTimeInDays {
            get { return this.Retrieve(x => x.PasswordExpirationTimeInDays, 30); }
            set { this.Store(x => x.PasswordExpirationTimeInDays, value); }
        }

        public MembershipPasswordFormat PasswordFormat {
            get { return this.Retrieve(x => x.PasswordFormat, MembershipPasswordFormat.Hashed); }
            set { this.Store(x => x.PasswordFormat, value); }
        }
    }
}