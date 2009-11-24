using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Tags {
    public class Permissions : IPermissionProvider {
        public static readonly Permission CreateTag = new Permission { Description = "Creating a Tag", Name = "CreateTag" };
        public static readonly Permission ApplyTag = new Permission { Description = "Applying a Tag", Name = "ApplyTag" };
        public static readonly Permission DeleteTag = new Permission { Description = "Deleting a Tag", Name = "DeleteTag" };
        public static readonly Permission RenameTag = new Permission { Description = "Renaming a Tag", Name = "RenameTag" };

        public string PackageName {
            get {
                return "Tags";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                CreateTag,
                ApplyTag,
                DeleteTag,
                RenameTag,
            };
        }
    }
}
