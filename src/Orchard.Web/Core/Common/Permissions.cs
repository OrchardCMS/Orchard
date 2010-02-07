using System;
using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ChangeOwner = new Permission { Name = "ChangeOwner", Description = "Change the owner of content items" };

        public string ModuleName {
            get { return "Common"; }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ChangeOwner,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrators",
                    Permissions = new[] {ChangeOwner}
                }
            };
        }
    }
}
