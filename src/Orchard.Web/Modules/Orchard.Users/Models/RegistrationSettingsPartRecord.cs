using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;
using Orchard.Mvc;

namespace Orchard.Users.Models {
    public class RegistrationSettingsPartRecord : ContentPartRecord {
        [UIOptions(EnableWrapper = true, DisplayName = "Users can create new accounts on the site")]
        public virtual bool UsersCanRegister { get; set; }
        [UIOptions(EnableWrapper = true, DisplayName = "Users must verify their email address")]
        public virtual bool UsersMustValidateEmail { get; set; }
        [StringLength(255)]
        [UIOptions(EnableWrapper = true, Template = "MediumString", DisplayName = "Website public name", Description = "The name of your website as it will appear in the verification e-mail.", EnabledBy = "UsersMustValidateEmail")]
        public virtual string ValidateEmailRegisteredWebsite { get; set; }
        [StringLength(255)]
        [UIOptions(EnableWrapper = true, Template = "MediumString", DisplayName = "Contact Us E-Mail address", Description = "The e-mail address displayed in the verification e-mail for a Contact Us link. Leave empty for no link.", EnabledBy = "UsersMustValidateEmail")]
        public virtual string ValidateEmailContactEMail { get; set; }

        [UIOptions(EnableWrapper = true, DisplayName = "Users must be approved before they can log in")]
        public virtual bool UsersAreModerated { get; set; }
        [UIOptions(EnableWrapper = true, DisplayName = "Send a notification when a user needs moderation", EnabledBy = "UsersAreModerated")]
        public virtual bool NotifyModeration { get; set; }
        [UIOptions(EnableWrapper = true, DisplayName = "Moderators", Description = "The usernames to send the notifications to (e.g., \"admin, user1, ...\").", EnabledBy = "NotifyModeration")]
        public virtual string NotificationsRecipients { get; set; }

        [UIOptions(EnableWrapper = true, DisplayName = "Display a link to enable users to reset their password")]
        public virtual bool EnableLostPassword { get; set; }
    }
}