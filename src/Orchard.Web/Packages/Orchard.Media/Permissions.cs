using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace  Orchard.Media {
    public class Permissions : IPermissionProvider {
        public static readonly Permission UploadMedia = new Permission { Description = "Uploading Media Files", Name = "UploadMedia" };
        public static readonly Permission ModifyMedia = new Permission { Description = "Modifying Media Files", Name = "ModifyMedia" };
        public static readonly Permission DeleteMedia = new Permission { Description = "Deleting Media Files", Name = "DeleteMedia" };
        public static readonly Permission CreateMediaFolder = new Permission { Description = "Creating Media Folders", Name = "CreateMediaFolder" };
        public static readonly Permission DeleteMediaFolder = new Permission { Description = "Deleting Media Folders", Name = "DeleteMediaFolder" };
        public static readonly Permission RenameMediaFolder = new Permission { Description = "Renaming Media Folders", Name = "RenameMediaFolder" };
   
        public string PackageName {
            get {
                return "Media";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                UploadMedia,
                ModifyMedia,
                DeleteMedia,
                CreateMediaFolder,
                DeleteMediaFolder,
                RenameMediaFolder
            };
        }
    }
}