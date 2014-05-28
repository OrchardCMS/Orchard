using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.AuditTrail {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewAuditTrail = new Permission { Description = "View Audit Trail", Name = "ViewAuditTrail" };
        public static readonly Permission ManageAuditTrailSettings = new Permission { Description = "Manage audit trail settings", Name = "ManageAuditTrailSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ViewAuditTrail;
            yield return ManageAuditTrailSettings;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ViewAuditTrail, ManageAuditTrailSettings}
                }
            };
        }
    }
}