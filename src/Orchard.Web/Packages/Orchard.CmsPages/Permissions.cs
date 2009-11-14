using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.CmsPages {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewPagesPermission = new Permission { Description = "Viewing CMS Pages", Name = "ViewPagesPermission" };
        public static readonly Permission CreatePagesPermission = new Permission { Description = "Creating CMS Pages", Name = "CreatePagesPermission" };
        public static readonly Permission CreateDraftPagesPermission = new Permission { Description = "Creating CMS Page Drafts", Name = "CreateDraftPagesPermission" };
        public static readonly Permission DeleteDraftPagesPermission = new Permission { Description = "Deleting CMS Page Drafts", Name = "DeleteDraftPagesPermission" };
        public static readonly Permission ModifyPagesPermission = new Permission { Description = "Modifying CMS Pages", Name = "ModifyPagesPermission" };
        public static readonly Permission DeletePagesPermission = new Permission { Description = "Deleting CMS Pages", Name = "DeletePagesPermission" };
        public static readonly Permission PublishPagesPermission = new Permission { Description = "Publishing CMS Pages", Name = "PublishPagesPermission" };
        public static readonly Permission UnpublishPagesPermission = new Permission { Description = "Unpublishing CMS Pages", Name = "UnpublishPagesPermission" };
        public static readonly Permission SchedulePagesPermission = new Permission { Description = "Scheduling CMS Pages", Name = "SchedulePagesPermission" };

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
                DeleteDraftPagesPermission,
                ModifyPagesPermission,
                DeletePagesPermission,
                PublishPagesPermission,
                UnpublishPagesPermission,
                SchedulePagesPermission
            };
        }
   }
}