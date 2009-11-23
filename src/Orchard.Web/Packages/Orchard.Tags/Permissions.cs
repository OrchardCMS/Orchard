using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Tags {
    public class Permissions : IPermissionProvider {
        public static readonly Permission CreateTag = new Permission { Description = "Creating a Tag", Name = "CreateTag" };

        public string PackageName {
            get {
                return "Tags";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                CreateTag,
            };
        }
    }
}
