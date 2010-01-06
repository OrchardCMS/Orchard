using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Pages {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewPages = new Permission { Description = "Viewing Pages", Name = "ViewPages" };
        public static readonly Permission CreatePages = new Permission { Description = "Creating Pages", Name = "CreatePages" };
        public static readonly Permission CreateDraftPages = new Permission { Description = "Creating Page Drafts", Name = "CreateDraftPages" };
        public static readonly Permission DeleteDraftPages = new Permission { Description = "Deleting Page Drafts", Name = "DeleteDraftPages" };
        public static readonly Permission ModifyPages = new Permission { Description = "Modifying Pages", Name = "ModifyPages" };
        public static readonly Permission DeletePages = new Permission { Description = "Deleting Pages", Name = "DeletePages" };
        public static readonly Permission PublishPages = new Permission { Description = "Publishing Pages", Name = "PublishPages" };
        public static readonly Permission UnpublishPages = new Permission { Description = "Unpublishing Pages", Name = "UnpublishPages" };
        public static readonly Permission SchedulePages = new Permission { Description = "Scheduling Pages", Name = "SchedulePages" };

        public string PackageName {
            get {
                return "NewPages";
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