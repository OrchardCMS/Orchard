using Orchard.ContentManagement;
using System;

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

        public bool UsersAreModerated {
            get { return Record.UsersAreModerated; }
            set { Record.UsersAreModerated = value; }
        }

        public bool NotifyModeration {
            get { return Record.NotifyModeration; }
            set { Record.NotifyModeration = value; }
        }

        public bool EnableLostPassword {
            get { return Record.EnableLostPassword; }
            set { Record.EnableLostPassword = value; }
        }

    }
}