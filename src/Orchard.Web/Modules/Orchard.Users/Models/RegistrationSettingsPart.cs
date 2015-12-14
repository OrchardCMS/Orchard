using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class RegistrationSettingsPart : ContentPart {
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

    }
}