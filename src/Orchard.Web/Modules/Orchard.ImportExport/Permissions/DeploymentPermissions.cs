using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ImportExport.Permissions {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentPermissions : IPermissionProvider {
        public static readonly Permission ExportToDeploymentTargets = new Permission {
            Description = "Export to deployment targets",
            Name = "ExportToDeploymentTargets"
        };

        public static readonly Permission ImportFromDeploymentSources = new Permission {
            Description = "Import from deployment sources",
            Name = "ImportFromDeploymentSources"
        };

        public static readonly Permission ConfigureDeployments = new Permission {
            Description = "Configure Deployments", 
            Name = "ConfigureDeployments"
        };

        public static readonly Permission ViewDeploymentHistory = new Permission {
            Description = "View deployment history",
            Name = "ViewDeploymentHistory",
            ImpliedBy = new[] {ConfigureDeployments}
        };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ExportToDeploymentTargets,
                ImportFromDeploymentSources,
                ConfigureDeployments,
                ViewDeploymentHistory
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ExportToDeploymentTargets, ImportFromDeploymentSources, ConfigureDeployments}
                },
                new PermissionStereotype {
                    Name = "Remote Orchard Publisher",
                    Permissions = new[] {ExportToDeploymentTargets, ImportExportPermissions.Export}
                },
                new PermissionStereotype {
                    Name = "Remote Orchard Subscriber",
                    Permissions = new[] {ImportFromDeploymentSources, ImportExportPermissions.Import}
                }
            };
        }
    }
}