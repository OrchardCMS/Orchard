using System;

namespace Orchard.Security {
    public class PasswordHistoryEntry : PasswordContext {
        public DateTime? LastPasswordChangeUtc { get; set; }
    }
}