using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {

    public class UsersIndexViewModel : BaseViewModel {
        public class Row {
            public UserPart UserPart { get; set; }
        }

        public IList<Row> Rows { get; set; }
    }
}
