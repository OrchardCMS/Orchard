using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Roles.Models;

namespace Orchard.Roles.ViewModels {
    public class RolesIndexViewModel : BaseViewModel {
        public IList<RoleRecord> Rows { get; set; }
    }
}
