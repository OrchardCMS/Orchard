using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class RegistrationSettingsPart : ContentPart<RegistrationSettingsPartRecord> {
        public bool UsersCanRegister {
            get { return Record.UsersCanRegister; }
            set { Record.UsersCanRegister = value; }
        }

        public bool UsersMustValidateEmail {
            get { return Record.UsersMustValidateEmail; }
            set { Record.UsersMustValidateEmail = value; }
        }

        public string ValidateEmailRegisteredWebsite {
            get { return Record.ValidateEmailRegisteredWebsite; }
            set { Record.ValidateEmailRegisteredWebsite = value; }
        }
        
        public string ValidateEmailContactEMail {
            get { return Record.ValidateEmailContactEMail; }
            set { Record.ValidateEmailContactEMail = value; }
        }

        public bool UsersAreModerated {
            get { return Record.UsersAreModerated; }
            set { Record.UsersAreModerated = value; }
        }

        public bool NotifyModeration {
            get { return Record.NotifyModeration; }
            set { Record.NotifyModeration = value; }
        }

        public string NotificationsRecipients {
            get { return Record.NotificationsRecipients; }
            set { Record.NotificationsRecipients = value; }
        }

        public bool EnableLostPassword {
            get { return Record.EnableLostPassword; }
            set { Record.EnableLostPassword = value; }
        }

    }
}