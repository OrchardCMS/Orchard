using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Mvc.ViewModels;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {

    public class UsersIndexViewModel : AdminViewModel {
        public class Row {
            public UserModel User { get; set; }
        }

        public IList<Row> Rows { get; set; }
    }
}
