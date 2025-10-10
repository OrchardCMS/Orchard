using System.Collections.Generic;

namespace Orchard.Roles.Constants {
    public static class SystemRoles {
        public const string Anonymous = nameof(Anonymous);
        public const string Authenticated = nameof(Authenticated);

        public static IEnumerable<string> GetSystemRoles() {
            return new List<string> { Anonymous, Authenticated };
        }
    }
}