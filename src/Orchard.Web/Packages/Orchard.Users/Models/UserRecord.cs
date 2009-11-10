using Orchard.Models.Records;

namespace Orchard.Users.Models {
    public class UserRecord : ModelPartRecord {
        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
    }
}