using Orchard.Security;

namespace Orchard.Users.Events {
    public class UserContext {
        public IUser User { get; set; }
        public bool Cancel { get; set; }
    }
}