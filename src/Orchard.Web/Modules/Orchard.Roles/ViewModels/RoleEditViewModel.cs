using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Mvc.ViewModels;
using Orchard.Security.Permissions;

namespace Orchard.Roles.ViewModels {
    public class RoleEditViewModel : BaseViewModel {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public IDictionary<string, IEnumerable<Permission>> ModulePermissions { get; set; }
        public IEnumerable<string> CurrentPermissions { get; set; }
        public IEnumerable<string> EffectivePermissions { get; set; }
    }
}
