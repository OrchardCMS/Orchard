using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.Users.Models {
    public sealed class User : ContentPart<UserRecord>, IUser {
        public readonly static ContentType ContentType = new ContentType { Name = "user", DisplayName = "User Profile" };

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
