using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Contents {
    public class Permissions : IPermissionProvider {
        public static readonly Permission CreateContentTypes = new Permission { Name = "CreateContentTypes", Description = "Create custom content types." };
        public static readonly Permission EditContentTypes = new Permission { Name = "EditContentTypes", Description = "Edit content types." };

        public string ModuleName {
            get { return "Contents"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new [] {
                CreateContentTypes,
                EditContentTypes,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}
