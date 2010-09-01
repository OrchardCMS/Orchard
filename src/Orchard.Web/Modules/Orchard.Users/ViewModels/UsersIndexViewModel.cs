using System.Collections.Generic;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {

    public class UsersIndexViewModel  {
        public class Row {
            public UserPart UserPart { get; set; }
        }

        public IList<Row> Rows { get; set; }
    }
}
