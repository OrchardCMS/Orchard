using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;
using Orchard.Mvc;

namespace Orchard.Users.Models {
    [UIOptions(DisplayName = "Users", EnableWrapper = true)]
    public class RegistrationSettingsPartRecord : ContentPartRecord {
        [UIOptions(Position = "1", DisplayName = "Users can create new accounts on the site")]
        public virtual bool UsersCanRegister { get; set; }

        [UIOptions(Position = "2", DisplayName = "Display a link to enable users to reset their password")]
        public virtual bool EnableLostPassword { get; set; }

        [UIOptions(Position = "3", DisplayName = "Users must verify their email address")]
        public virtual bool UsersMustValidateEmail { get; set; }

        [StringLength(255)]
        [UIOptions(Position = "4", Template = "MediumString", DisplayName = "Website public name", Description = "The name of your website as it will appear in the verification e-mail.", EnabledBy = "UsersMustValidateEmail")]
        public virtual string ValidateEmailRegisteredWebsite { get; set; }

        [StringLength(255)]
        [UIOptions(Position = "5", Template = "MediumString", DisplayName = "Contact Us E-Mail address", Description = "The e-mail address displayed in the verification e-mail for a Contact Us link. Leave empty for no link.", EnabledBy = "UsersMustValidateEmail")]
        public virtual string ValidateEmailContactEMail { get; set; }

        [UIOptions(Position = "6", DisplayName = "Users must be approved before they can log in")]
        public virtual bool UsersAreModerated { get; set; }

        [UIOptions(Position = "7", DisplayName = "Send a notification when a user needs moderation", EnabledBy = "UsersAreModerated")]
        public virtual bool NotifyModeration { get; set; }

        [UIOptions(Position = "8", DisplayName = "Moderators", Description = "The usernames to send the notifications to (e.g., \"admin, user1, ...\").", EnabledBy = "NotifyModeration")]
        public virtual string NotificationsRecipients { get; set; }

    }
}