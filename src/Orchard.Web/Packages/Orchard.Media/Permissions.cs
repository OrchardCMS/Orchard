using System.Collections.Generic;
using System.Linq;
using Orchard.Security.Permissions;

namespace  Orchard.Media {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageMediaFiles = new Permission { Description = "Modifying Media Files", Name = "ManageMediaFiles" };
        public static readonly Permission UploadMediaFiles = new Permission { Description = "Uploading Media Files", Name = "UploadMediaFiles" };
   
        public string PackageName {
            get {
                return "Media";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new Permission[] {
                ManageMediaFiles,
                UploadMediaFiles,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }

    }
}