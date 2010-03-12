using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;
using Orchard.Security.Permissions;

namespace Orchard.Roles.ViewModels {
    public class RoleCreateViewModel : BaseViewModel {
        [Required]
        public string Name { get; set; }
        public IDictionary<string, IEnumerable<Permission>> ModulePermissions { get; set; }
    }
}
