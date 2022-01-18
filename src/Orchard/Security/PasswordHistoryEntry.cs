using System;

namespace Orchard.Security {
    public class PasswordHistoryEntry : PasswordContext {
        public IUser User { get; set; }
        public DateTime? LastPasswordChangeUtc { get; set; }
    }
}