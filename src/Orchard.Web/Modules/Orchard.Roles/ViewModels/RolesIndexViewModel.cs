using System.Collections.Generic;
using Orchard.Roles.Models;

namespace Orchard.Roles.ViewModels {
    public class RolesIndexViewModel  {
        public IList<RoleRecord> Rows { get; set; }
    }
}
