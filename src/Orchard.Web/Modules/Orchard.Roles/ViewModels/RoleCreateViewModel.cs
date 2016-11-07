using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Security.Permissions;

namespace Orchard.Roles.ViewModels {
    public class RoleCreateViewModel  {
        [Required, StringLength(255)]
        public string Name { get; set; }
        public IDictionary<string, IEnumerable<Permission>> FeaturePermissions { get; set; }
    }
}
