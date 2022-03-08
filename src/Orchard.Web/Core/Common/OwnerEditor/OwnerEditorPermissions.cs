using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Core.Common.OwnerEditor {
    public class OwnerEditorPermissions : IPermissionProvider {

        public static readonly Permission MayEditContentOwner = new Permission {
            Description = "Edit the Owner of content items",
            Name = "MayEditContentOwner",
            ImpliedBy = new[] { StandardPermissions.SiteOwner } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { MayEditContentOwner }
                } };
        }

        public IEnumerable<Permission> GetPermissions() {
            return new[] { MayEditContentOwner };
        }
    }
}