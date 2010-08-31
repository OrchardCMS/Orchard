using Orchard.Users.Models;

namespace Orchard.Users.Events {
    public class UserContext {
        public UserPart User { get; set; }
        public bool Cancel { get; set; }
    }
}