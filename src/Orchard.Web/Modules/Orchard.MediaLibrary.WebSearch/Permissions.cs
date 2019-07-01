using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.MediaLibrary.WebSearch {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageWebSearchMediaContent =
            new Permission { Description = "Manage Web Search Media", Name = nameof(ManageWebSearchMediaContent) };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() =>
            new[] {
                ManageWebSearchMediaContent
            };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageWebSearchMediaContent }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageWebSearchMediaContent }
                },
            };

    }
}