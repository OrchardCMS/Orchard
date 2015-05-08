using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Dashboards {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageDashboards = new Permission { Description = "Manage dashboards", Name = "ManageDashboards" };
        public static readonly Permission ManageOwnDashboard = new Permission { Description = "Manage your own dashboard", Name = "ManageOwnDashboard", ImpliedBy = new[] { ManageDashboards } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ManageDashboards;
            yield return ManageOwnDashboard;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            yield return new PermissionStereotype {
                Name = "Administrator",
                Permissions = new[] {
                    ManageDashboards
                }
            };
        }
    }
}