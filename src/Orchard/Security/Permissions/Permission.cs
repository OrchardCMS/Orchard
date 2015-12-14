using System.Collections.Generic;

namespace Orchard.Security.Permissions {
    public class Permission {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public IEnumerable<Permission> ImpliedBy { get; set; }
        public bool RequiresOwnership { get; set; }

        public static Permission Named(string name) {
            return new Permission { Name = name };
        }
    }
}