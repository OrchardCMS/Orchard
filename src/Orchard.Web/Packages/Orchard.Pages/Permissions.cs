using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace Orchard.Pages {
    public class Permissions : IPermissionProvider {
        public static readonly Permission EditPages = new Permission { Description = "Edit page", Name = "EditPages" };
        public static readonly Permission EditOthersPages = new Permission { Description = "Edit page for others", Name = "EditOthersPages" };
        public static readonly Permission PublishPages = new Permission { Description = "Publish or unpublish page", Name = "PublishPages" };
        public static readonly Permission PublishOthersPages = new Permission { Description = "Publish or unpublish page for others", Name = "PublishOthersPages" };
        public static readonly Permission DeletePages = new Permission { Description = "Delete page", Name = "DeletePages" };
        public static readonly Permission DeleteOthersPages = new Permission { Description = "Delete page for others", Name = "DeleteOthersPages" };

        public string PackageName {
            get {
                return "Pages";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                EditPages,
                EditOthersPages,
                PublishPages,
                PublishOthersPages,
                DeletePages,
                DeleteOthersPages,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}