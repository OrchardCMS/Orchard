using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Roles.Models;

namespace Orchard.Roles.ViewModels {
    public class RolesIndexViewModel : AdminViewModel {
        public class Row {
            public RoleRecord Role { get; set; }
        }

        public IList<Row> Rows { get; set; }
    }
}
