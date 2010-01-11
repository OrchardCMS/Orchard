using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}