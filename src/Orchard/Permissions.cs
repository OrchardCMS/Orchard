using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard {
    public class Permissions : IPermissionProvider {
        public static readonly Permission AccessAdmin = new Permission { Name = "AccessAdmin", Description = "Access the application admin area" };

        public string PackageName {
            get { return "Orchard"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { AccessAdmin };
        }
    }
}