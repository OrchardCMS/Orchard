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
    }
}