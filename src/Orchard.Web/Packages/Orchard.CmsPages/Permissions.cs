using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.CmsPages {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewPages = new Permission { Description = "Viewing CMS Pages", Name = "ViewPages" };
        public static readonly Permission CreatePages = new Permission { Description = "Creating CMS Pages", Name = "CreatePages" };
        public static readonly Permission CreateDraftPages = new Permission { Description = "Creating CMS Page Drafts", Name = "CreateDraftPages" };
        public static readonly Permission DeleteDraftPages = new Permission { Description = "Deleting CMS Page Drafts", Name = "DeleteDraftPages" };
        public static readonly Permission ModifyPages = new Permission { Description = "Modifying CMS Pages", Name = "ModifyPages" };
        public static readonly Permission DeletePages = new Permission { Description = "Deleting CMS Pages", Name = "DeletePages" };
        public static readonly Permission PublishPages = new Permission { Description = "Publishing CMS Pages", Name = "PublishPages" };
        public static readonly Permission UnpublishPages = new Permission { Description = "Unpublishing CMS Pages", Name = "UnpublishPages" };
        public static readonly Permission SchedulePages = new Permission { Description = "Scheduling CMS Pages", Name = "SchedulePages" };

        public string PackageName {
            get {
                return "CmsPages";
            }
        }

        public IEnumerable<Permission> GetPermissions() {
            return new List<Permission> {
                ViewPages,
                CreatePages,
                CreateDraftPages,
                DeleteDraftPages,
                ModifyPages,
                DeletePages,
                PublishPages,
                UnpublishPages,
                SchedulePages
            };
        }
   }
}