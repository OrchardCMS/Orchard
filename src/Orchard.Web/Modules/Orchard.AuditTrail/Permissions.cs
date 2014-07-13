using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.AuditTrail {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewAuditTrail = new Permission { Description = "View audit trail", Name = "ViewAuditTrail" };
        public static readonly Permission ManageAuditTrailSettings = new Permission { Description = "Manage audit trail settings", Name = "ManageAuditTrailSettings" };
        public static readonly Permission ImportAuditTrail = new Permission { Description = "Import audit trail", Name = "ImportAuditTrail" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ViewAuditTrail;
            yield return ManageAuditTrailSettings;
            yield return ImportAuditTrail;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            yield return new PermissionStereotype {
                Name = "Administrator",
                Permissions = new[] {
                    ViewAuditTrail, 
                    ManageAuditTrailSettings,
                    /* Not even an administrator will get the ImportAuditTrail permission. */
                }
            };
        }
    }
}