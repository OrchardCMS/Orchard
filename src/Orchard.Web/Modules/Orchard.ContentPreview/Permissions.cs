using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ContentPreview {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ContentPreview = new Permission { Name = "ContentPreview", Description = "Display content preview" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { ContentPreview };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ContentPreview }
                }
            };
        }
    }
}