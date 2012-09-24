using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.Users.Models {
    public sealed class UserPart : ContentPart<UserPartRecord>, IUser {
        public const string EmailPattern = 
            @"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

        public string UserName {
            get { return Record.UserName; }
            set { Record.UserName = value; }
        }

        public string Email {
            get { return Record.Email; }
            set { Record.Email = value; }
        }

        public string NormalizedUserName {
            get { return Record.NormalizedUserName; }
            set { Record.NormalizedUserName = value; }
        }

        public UserStatus RegistrationStatus {
            get { return Record.RegistrationStatus; }
            set { Record.RegistrationStatus = value; }
        }

        public UserStatus EmailStatus {
            get { return Record.EmailStatus; }
            set { Record.EmailStatus = value; }
        }
    }
}
