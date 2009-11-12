using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.CmsPages {
    public class CmsPagesPermissionsProvider : IPermissionProvider {
        public static readonly Permission ViewPagesPermission = new Permission { Description = "Viewing CMS Pages", Name = "ViewCmsPagesPermission" };
        public static readonly Permission CreatePagesPermission = new Permission { Description = "Creating CMS Pages", Name = "CreateCmsPagesPermission" };
        public static readonly Permission CreateDraftPagesPermission = new Permission { Description = "Creating CMS Page Drafts", Name = "CreateDraftCmsPagesPermission" };
        public static readonly Permission ModifyPagesPermission = new Permission { Description = "Modifying CMS Pages", Name = "ModifyCmsPagesPermission" };
        public static readonly Permission DeletePagesPermission = new Permission { Description = "Deleting CMS Pages", Name = "DeleteCmsPagesPermission" };
        public static readonly Permission PublishPagesPermission = new Permission { Description = "Publishing CMS Pages", Name = "PublishCmsPagesPermission" };
        public static readonly Permission UnpublishPagesPermission = new Permission { Description = "Unpublishing CMS Pages", Name = "UnpublishCmsPagesPermission" };
        public static readonly Permission SchedulePagesPermission = new Permission { Description = "Scheduling CMS Pages", Name = "ScheduleCmsPagesPermission" };

        #region Implementation of IPermissionProvider

        public string PackageName {
            get {
                return "CmsPages";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                ViewPagesPermission,
                CreatePagesPermission,
                CreateDraftPagesPermission,
                ModifyPagesPermission,
                DeletePagesPermission,
                PublishPagesPermission,
                UnpublishPagesPermission,
                SchedulePagesPermission
            };
        }

        #endregion
    }
}