using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.AuditTrail {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageAuditTrail = new Permission { Description = "Manage Audit Trail", Name = "ManageAuditTrail" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ManageAuditTrail;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageAuditTrail}
                }
            };
        }
    }
}