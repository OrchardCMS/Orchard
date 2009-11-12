using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace  Orchard.Media {
    public class MediaPermissionsProvider : IPermissionProvider {
        public static readonly Permission UploadMediaPermission = new Permission { Description = "Uploading Media Files", Name = "UploadMediaPermission" };
        public static readonly Permission ModifyMediaPermission = new Permission { Description = "Modifying Media Files", Name = "ModifyMediaPermission" };
        public static readonly Permission DeleteMediaPermission = new Permission { Description = "Deleting Media Files", Name = "DeleteMediaPermission" };
        public static readonly Permission CreateMediaFolderPermission = new Permission { Description = "Creating Media Folders", Name = "CreateMediaFolderPermission" };
        public static readonly Permission DeleteMediaFolderPermission = new Permission { Description = "Deleting Media Folders", Name = "DeleteMediaFolderPermission" };
        public static readonly Permission RenameMediaFolderPermission = new Permission { Description = "Renaming Media Folders", Name = "RenameMediaFolderPermission" };

        #region Implementation of IPermissionProvider

        public string PackageName {
            get {
                return "Media";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                UploadMediaPermission,
                ModifyMediaPermission,
                DeleteMediaPermission,
                CreateMediaFolderPermission,
                DeleteMediaFolderPermission,
                RenameMediaFolderPermission
            };
        }

        #endregion
    }
}