using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.Users.Models {
    public sealed class UserPart : ContentPart<UserPartRecord>, IUser {
        public const string EmailPattern = 
            @"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@([a-z0-9][\w-]*\.)+[a-z]{2,}$";

        public string UserName {
            get { return Retrieve(x => x.UserName); }
            set { Store(x => x.UserName, value); }
        }

        public string Email {
            get { return Retrieve(x => x.Email); }
            set { Store(x => x.Email, value); }
        }

        public string NormalizedUserName {
            get { return Retrieve(x => x.NormalizedUserName); }
            set { Store(x => x.NormalizedUserName, value); }
        }

        public UserStatus RegistrationStatus {
            get { return Retrieve(x => x.RegistrationStatus); }
            set { Store(x => x.RegistrationStatus, value); }
        }

        public UserStatus EmailStatus {
            get { return Retrieve(x => x.EmailStatus); }
            set { Store(x => x.EmailStatus, value); }
        }
    }
}
