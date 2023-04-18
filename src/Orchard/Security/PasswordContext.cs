using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Orchard.Security {
    public class PasswordContext {
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string HashAlgorithm { get; set; }
        public MembershipPasswordFormat PasswordFormat { get; set; }

        // In some rare cases, it's important to carry information about a user
        // this password belongs to. A practical example is when we have to force
        // an upgrade of the hashing/encryption scheme used for the password, and
        // store corresponding information.
        public IUser User { get; set; }
    }
}