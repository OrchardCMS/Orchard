using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Azure.MediaServices {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageCloudMediaContent = new Permission { Description = "Managing Microsoft Azure Media", Name = "ManageCloudMediaContent" };
        public static readonly Permission ManageCloudMediaJobs = new Permission { Description = "Managing Microsoft Azure Media Jobs", Name = "ManageCloudMediaJobs" };
        public static readonly Permission ManageCloudMediaSettings = new Permission { Description = "Managing Microsoft Azure Media Settings", Name = "ManageCloudMediaSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            yield return ManageCloudMediaContent;
            yield return ManageCloudMediaJobs;
            yield return ManageCloudMediaSettings;
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            yield return new PermissionStereotype {
                Name = "Administrator",
                Permissions = GetPermissions().ToArray()
            };
            yield return new PermissionStereotype {
                Name = "Editor",
                Permissions = new[] {ManageCloudMediaContent}
            };
            yield return new PermissionStereotype {
                Name = "Moderator",
            };
            yield return new PermissionStereotype {
                Name = "Author",
                Permissions = new[] {ManageCloudMediaContent}
            };
            yield return new PermissionStereotype {
                Name = "Contributor",
            };
        }

    }
}