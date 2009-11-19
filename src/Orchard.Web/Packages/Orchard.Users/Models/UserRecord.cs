using System.Web.Security;
using Orchard.Models.Records;

namespace Orchard.Users.Models {
    public class UserRecord : ContentPartRecord {
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }

        public virtual string Password { get; set; }
        public virtual MembershipPasswordFormat PasswordFormat { get; set; }
        public virtual string PasswordSalt { get; set; }
    }
}