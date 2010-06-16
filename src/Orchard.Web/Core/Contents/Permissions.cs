using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class Permissions : IPermissionProvider {
        public static readonly Permission CreateContentType = new Permission { Name = "CreateContentType", Description = "Create custom content type." };

        public string ModuleName {
            get { return "Contents"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                CreateContentType,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {CreateContentType}
                }
            };
        }
    }
}
