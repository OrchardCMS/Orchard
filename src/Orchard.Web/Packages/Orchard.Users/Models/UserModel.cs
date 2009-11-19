using Orchard.Models;
using Orchard.Security;

namespace Orchard.Users.Models {
    public sealed class UserModel : ContentItemPartWithRecord<UserRecord>, IUser {
        public int Id {
            get { return ContentItem.Id; }
        }

        public string UserName {
            get { return Record.UserName; }
            set { Record.UserName = value; }
        }

        public string Email {
            get { return Record.Email; }
            set { Record.Email = value; }
        }
    }
}
