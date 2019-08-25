using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ContentPreview {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ContentPreview =
            new Permission { Name = nameof(ContentPreview), Description = "Display content preview" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() => new[] { ContentPreview };

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ContentPreview }
                }
            };
    }
}