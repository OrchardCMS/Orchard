using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.MetaData {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMetaData = new Permission { Description = "Manage MetaData", Name = "ManageMetaData" };//q: Should edit_MetaData be ManageMetaData?


        public string ModuleName {
            get {
                return "MetaData";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageMetaData,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageMetaData}
                },
            };
        }

    }
}


