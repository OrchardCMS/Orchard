using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Users.Models {
    public class UserModel : ModelPart, IUser {
        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
